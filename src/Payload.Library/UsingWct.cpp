#include "stdafx.h"
#include "Payload.Dll.h"
#include "WinApiException.h"

#pragma comment(lib, "Psapi.lib")
#pragma comment(lib, "Advapi32.lib")

BOOL GrantDebugPrivilege ( )
/*++

Routine Description:

    Enables the debug privilege (SE_DEBUG_NAME) for this process.
    This is necessary if we want to retrieve wait chains for processes
    not owned by the current user.

Arguments:

    None.

Return Value:

    TRUE if this privilege could be enabled; FALSE otherwise.

--*/
{
    BOOL             fSuccess    = FALSE;
    HANDLE           TokenHandle = NULL;
    TOKEN_PRIVILEGES TokenPrivileges;

    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &TokenHandle))
    {
        printf("Could not get the process token");
        goto Cleanup;
    }

    TokenPrivileges.PrivilegeCount = 1;

    if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &TokenPrivileges.Privileges[0].Luid))
    {
        printf("Couldn't lookup SeDebugPrivilege name");
        goto Cleanup;
    }

    TokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    if (!AdjustTokenPrivileges(TokenHandle, FALSE, &TokenPrivileges, sizeof(TokenPrivileges), NULL, NULL))
    {
        printf("Could not revoke the debug privilege");
        goto Cleanup;
    }

    fSuccess = TRUE;

 Cleanup:

    if (TokenHandle)
    {
        CloseHandle(TokenHandle);
    }

    return fSuccess;
}

TCHAR* ConhostImageFileName = TEXT("\\Device\\HarddiskVolume1\\Windows\\System32\\conhost.exe");

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

    // Create WCT session
    HWCT wctSession = OpenThreadWaitChainSession(0, NULL);
    if( NULL == wctSession ) { throw WinApiException("OpenThreadWaitChainSession"); }
    shared_ptr<void> wctSession_deleter( wctSession, CloseThreadWaitChainSession );

    // Try to enable the SE_DEBUG_NAME privilege for this process. 
    // Continue even if this fails--we just won't be able to retrieve
    // wait chains for processes not owned by the current user.
    if (!GrantDebugPrivilege())
    {
        printf("Could not enable the debug privilege");
    }

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
