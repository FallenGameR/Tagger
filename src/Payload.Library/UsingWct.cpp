#include "stdafx.h"
#include "Payload.Dll.h"
#include "WinApiException.h"

TCHAR* ConhostImageFileName = TEXT("\\Device\\HarddiskVolume1\\Windows\\System32\\conhost.exe");

void GrantDebugPrivilege ( )
{
    HANDLE token;
    BOOL successOpen = OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY | TOKEN_ADJUST_PRIVILEGES, &token);
    if( !successOpen ) { throw WinApiException("OpenProcessToken"); }
    shared_ptr<void> token_deleter( token, CloseHandle );

    TOKEN_PRIVILEGES privileges;
    privileges.PrivilegeCount = 1;
    privileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    BOOL successLookup = LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &privileges.Privileges[0].Luid);
    if( !successLookup ) { throw WinApiException("LookupPrivilegeValue"); }

    BOOL successAdjust = AdjustTokenPrivileges(token, FALSE, &privileges, sizeof(privileges), NULL, NULL);
    if( !successAdjust ) { throw WinApiException("AdjustTokenPrivileges"); }
}

bool IsConhost( DWORD pid )
{
    HANDLE process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid);
    if( NULL == process ) { throw WinApiException("OpenProcess"); }
    shared_ptr<void> process_deleter( process, CloseHandle );

    TCHAR imageFileName[MAX_PATH];
    DWORD length = GetProcessImageFileName(process, imageFileName, MAX_PATH);
    if( 0 == length ) { throw WinApiException("GetProcessImageFileName"); }

    return 0 == _tcsicmp( imageFileName, ConhostImageFileName );    
}

DWORD FindParentConhost( HWCT wctSession, DWORD tid )
{
    WAITCHAIN_NODE_INFO nodes[WCT_MAX_NODE_COUNT];
    DWORD count = WCT_MAX_NODE_COUNT;
    BOOL isCycle;

    // Synchronous WCT call to retrieve wait chain
    BOOL success = GetThreadWaitChain( wctSession, NULL, WCT_OUT_OF_PROC_FLAG, tid, &count, nodes, &isCycle );
    if( !success ) { throw WinApiException("GetThreadWaitChain"); }

    // Check if thread waits on conhost process
    for ( DWORD i = 0; i < count; i++)
    {
        if( WctThreadType != nodes[i].ObjectType ) { continue; }
        DWORD pid = nodes[i].ThreadObject.ProcessId;
        if( IsConhost( pid ) ) { return pid; }
    }

    // Return 0 if conhost wasn't found
    return 0;
}

///
/// ProcId
///     pid - wait chains for the specified process
///
HOOKDLL_API DWORD FindConhost( DWORD pid )
{
    // We only perform search for concrete process
    if( 0 == pid ) { return 0; }

    // Try to enable the SE_DEBUG_NAME privilege for this process. 
    // Continue even if this fails - we just won't be able to retrieve
    // wait chains for processes not owned by the current user.
    //GrantDebugPrivilege();

    // Create WCT session
    HWCT wctSession = OpenThreadWaitChainSession(0, NULL);
    if( NULL == wctSession ) { throw WinApiException("OpenThreadWaitChainSession"); }
    shared_ptr<void> wctSession_deleter( wctSession, CloseThreadWaitChainSession );

    // Get a snapshot of this process. This enables us to enumerate its threads.
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, pid);
    if( INVALID_HANDLE_VALUE == snapshot ) { throw WinApiException("CreateToolhelp32Snapshot"); } 
    shared_ptr<void> snapshot_deleter( snapshot, CloseHandle );

    // Initialize thread entry structure
    THREADENTRY32 threadEntry;
    threadEntry.dwSize = sizeof(threadEntry);
    if( !Thread32First(snapshot, &threadEntry) ) { return 0; }

    // Walk the thread list and print each wait chain
    do
    {
        if( threadEntry.th32OwnerProcessID != pid ) { continue; }

        // Open a handle to this specific thread
        HANDLE thread = OpenThread(THREAD_ALL_ACCESS, FALSE, threadEntry.th32ThreadID);
        if( NULL == thread ) { throw WinApiException("OpenThread"); }
        shared_ptr<void> thread_deleter( thread, CloseHandle );

        // Check whether the thread is still running
        DWORD exitCode;
        BOOL success = GetExitCodeThread(thread, &exitCode);
        if( !success ) { throw WinApiException("GetExitCodeThread"); }

        // Print wait chains for active threads
        if( STILL_ACTIVE == exitCode )
        {
            DWORD conhostPid = FindParentConhost( wctSession, threadEntry.th32ThreadID );
            if( 0 != conhostPid ) { return conhostPid; }
        }
    } 
    while( Thread32Next(snapshot, &threadEntry) );

    // Return 0 if conhost wasn't found
    return 0;
}
