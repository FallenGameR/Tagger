#include "stdafx.h"
#include "Payload.Dll.h"
#include "WinApiException.h"

#pragma comment(lib, "Psapi.lib")
#pragma comment(lib, "Advapi32.lib")

typedef struct _STR_ARRAY
{
    CHAR Desc[32];
} STR_ARRAY;

// Human-readable names for the different synchronization types.
STR_ARRAY STR_OBJECT_TYPE[] =
{
    {"CriticalSection"},
    {"SendMessage"},
    {"Mutex"},
    {"Alpc"},
    {"Com"},
    {"ThreadWait"},
    {"ProcWait"},
    {"Thread"},
    {"ComActivation"},
    {"Unknown"},
    {"Max"}
};

// Global variable to store the WCT session handle
HWCT g_WctHandle = NULL;

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

void PrintProcessName( DWORD pid )
{
    WCHAR file[MAX_PATH];

    HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    if( NULL == process ) { throw WinApiException("OpenProcess"); }

    // Retrieve the executable name and print it.
    if (GetProcessImageFileName(process, file, ARRAYSIZE(file)) > 0)
    {
        PCWSTR filePart = wcsrchr(file, L'\\');
        if (filePart)
        {
            filePart++;
        }
        else
        {
            filePart = file;
        }

        printf("%S", filePart);
    }
}

void PrintWaitChain ( __in DWORD tid )
{
    WAITCHAIN_NODE_INFO nodeInfoArray[WCT_MAX_NODE_COUNT];
    DWORD count, i;
    BOOL isCycle;

    printf("%d: ", tid);

    count = WCT_MAX_NODE_COUNT;

    // Make a synchronous WCT call to retrieve the wait chain.
    BOOL success = GetThreadWaitChain(g_WctHandle, NULL, WCTP_GETINFO_ALL_FLAGS, tid, &count, nodeInfoArray, &isCycle);
    if( !success ) { throw WinApiException("GetThreadWaitChain"); }

    // Check if the wait chain is too big for the array we passed in.
    if (count > WCT_MAX_NODE_COUNT)
    {
        printf("Found additional nodes %d\n", count);
        count = WCT_MAX_NODE_COUNT;
    }

    // Loop over all the nodes returned and print useful information.
    for (i = 0; i < count; i++)
    {
        if( WctThreadType == nodeInfoArray[i].ObjectType )
        {
            // A thread node contains process and thread ID.
            DWORD pid = nodeInfoArray[i].ThreadObject.ProcessId;
            printf("[pid %d = ", pid);
            PrintProcessName( pid );
            printf("] | ", pid);
        }
    }

    printf("\n");
}

void CheckThreads( __in DWORD pid )
{
    // Get a handle to this process.
    HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    if( NULL == process ) { throw WinApiException("OpenProcess"); }

    // Get a snapshot of this process. This enables us to enumerate its threads.
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, pid);
    if( INVALID_HANDLE_VALUE == snapshot ) { throw WinApiException("CreateToolhelp32Snapshot"); } 

    THREADENTRY32 thread;
    thread.dwSize = sizeof(thread);

    // Walk the thread list and print each wait chain
    if( !Thread32First(snapshot, &thread) ) { return; }

    do
    {
        if( thread.th32OwnerProcessID != pid ) { continue; }

        // Open a handle to this specific thread
        HANDLE threadHandle = OpenThread(THREAD_ALL_ACCESS, FALSE, thread.th32ThreadID);
        if( NULL == threadHandle ) { throw WinApiException("OpenThread"); }

        // Check whether the thread is still running
        DWORD exitCode;
        GetExitCodeThread(threadHandle, &exitCode);

        if( STILL_ACTIVE == exitCode )
        {
            PrintWaitChain(thread.th32ThreadID);
        }

        CloseHandle(threadHandle);
    } 
    while( Thread32Next(snapshot, &thread) );

    CloseHandle(snapshot);
    CloseHandle(process);
}

///
/// ProcId
///     0   - wait chains for all processes
///     pid - wait chains for the specified process
///
HOOKDLL_API int _cdecl UsingWctMain( DWORD pid )
{
    g_WctHandle = OpenThreadWaitChainSession(0, NULL);
    if( NULL == g_WctHandle ) { throw WinApiException("OpenThreadWaitChainSession"); }

    // Try to enable the SE_DEBUG_NAME privilege for this process. 
    // Continue even if this fails--we just won't be able to retrieve
    // wait chains for processes not owned by the current user.
    if (!GrantDebugPrivilege())
    {
        printf("Could not enable the debug privilege");
    }

    CheckThreads(pid);

    CloseThreadWaitChainSession(g_WctHandle);
    
    return 0;
}
