#include "stdafx.h"
#include "Payload.Dll.h"
#include "WinApiException.h"


//-------------------------------------------------------
// shared data 
// Notice:	seen by both: the instance of "HookSpy.dll" mapped
//			into the remote process as well as by the instance
//			of "HookSpy.dll" mapped into our "HookSpy.exe"
#pragma data_seg (".shared")
HWND    shared_wnd_handle	= 0;		// control containing the password
HHOOK   shared_hook = 0;
UINT    g_test = 0;
UINT    WM_USERMESSAGE = 0;
char    shared_string [128] = { '\0' };
#pragma data_seg ()

#pragma comment(linker,"/SECTION:.shared,RWS") 



//-------------------------------------------------------
// global variables (unshared!)
//
HINSTANCE hDll;

//-------------------------------------------------------
// DllMain
//
BOOL APIENTRY DllMain( HANDLE hModule, DWORD  ul_reason_for_call,  LPVOID lpReserved )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    
    hDll = (HINSTANCE) hModule;
    return TRUE;
}


//-------------------------------------------------------
// HookProc
// Notice: - executed by the "remote" instance of "HookSpy.dll";
//		   - unhooks itself right after it gets the password;
//
#define pCW ((CWPSTRUCT*)lParam)

LRESULT MyHookProcedure( int code, WPARAM wParam, LPARAM lParam )
{	
    if( pCW->message == WM_USERMESSAGE ) 
    {
        MessageBeep( MB_OK );
        SendMessage( shared_wnd_handle, WM_GETTEXT, 128, (LPARAM) shared_string );
        UnhookWindowsHookEx( shared_hook );
    }

    g_test = 42;

    return CallNextHookEx( shared_hook, code, wParam, lParam );
}


int APIENTRY InjectDll_HookMessageQueue( HWND handle, LPSTR outString )
{	
    AddDebugPrivilege();

    shared_wnd_handle = handle;

    DWORD processThreadId = GetWindowThreadProcessId( handle, NULL );

    //HMODULE test = GetCurrentModule();
    shared_hook = SetWindowsHookEx( WH_CALLWNDPROC, (HOOKPROC) MyHookProcedure, hDll, processThreadId );
    if( NULL == shared_hook ) { throw WinApiException("SetWindowsHookEx"); }
    
    //ULONG originalWindowProcedure = GetWindowLong( handle, GWL_WNDPROC );
    //if( 0 == originalWindowProcedure ) { throw WinApiException("GetWindowLong"); }
   
    WM_USERMESSAGE = RegisterWindowMessage( TEXT("WM_USERMESSAGE") );
    if( 0 == WM_USERMESSAGE ) { throw WinApiException("RegisterWindowMessage"); }

    SendMessage( handle, WM_USERMESSAGE, 0, 0 );
    //SendMessage( handle, WM_CLOSE, 0, 0 );

    if( 42 != g_test ) { throw WinApiException("MyHookProcedure didn't executed"); }

    strcpy( outString, shared_string );
    return strlen(outString);
}

void APIENTRY AddDebugPrivilege()
{ 
    HANDLE token;
    HANDLE injector = GetCurrentProcess();
    TOKEN_PRIVILEGES privileges;

    if( 0 == OpenProcessToken( injector, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &token ) ) { throw WinApiException("OpenProcessToken"); }
    if( 0 == LookupPrivilegeValue( NULL, SE_DEBUG_NAME, &privileges.Privileges[0].Luid ) ) { throw WinApiException("LookupPrivilegeValue"); }

    privileges.PrivilegeCount = 1;
    privileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 

    if( 0 == AdjustTokenPrivileges( token, 0, &privileges, sizeof(privileges), NULL, NULL ) ) { throw WinApiException("AdjustTokenPrivileges"); }
}   

void APIENTRY InjectDll_CreateRemoteThread( DWORD processId, LPCSTR dllPath )
{
    // TODO: This hook doesn't unload injected DLL itself. But it should.
    // TODO: Check what OpenProcess flags are really required.

    HANDLE hProcess = NULL;
    LPVOID lpBaseAddr = NULL;
    HMODULE hKernel32Dll = NULL;
    HANDLE hThread = NULL;

    try
    {
        hProcess = OpenProcess( PROCESS_CREATE_THREAD|PROCESS_QUERY_INFORMATION|PROCESS_VM_OPERATION|PROCESS_VM_WRITE|PROCESS_VM_READ, FALSE, processId );
        if( NULL == hProcess ) { throw WinApiException("OpenProcess"); }

        DWORD dwMemSize = lstrlenA(dllPath) + 1;
        lpBaseAddr = VirtualAllocEx( hProcess, NULL, dwMemSize, MEM_COMMIT, PAGE_READWRITE );
        if( NULL == lpBaseAddr ) { throw WinApiException("VirtualAllocEx"); }

        BOOL memoryWriteSuccess = WriteProcessMemory( hProcess, lpBaseAddr, dllPath, dwMemSize, NULL );
        if( 0 == memoryWriteSuccess ) { throw WinApiException("WriteProcessMemory"); }

        hKernel32Dll = LoadLibrary( TEXT("kernel32.dll") );
        if( NULL == hKernel32Dll ) { throw WinApiException("LoadLibrary"); }

        LPVOID lpFuncAddr = GetProcAddress( hKernel32Dll, "LoadLibraryA" );
        if( NULL == lpFuncAddr ) { throw WinApiException("GetProcAddress"); }

        hThread = CreateRemoteThread( hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)lpFuncAddr, lpBaseAddr, 0, NULL );
        if( NULL == hThread ) { throw WinApiException("CreateRemoteThread"); }

        DWORD waitResult = WaitForSingleObject( hThread, INFINITE );
        if( WAIT_FAILED == waitResult ) { throw WinApiException("WaitForSingleObject"); }
        if( WAIT_OBJECT_0 != waitResult ) { throw WinApiException("WaitForSingleObject returned", waitResult ); }

        DWORD dwExitCode;
        BOOL exitThreadResult = GetExitCodeThread( hThread, &dwExitCode );
        if( 0 == exitThreadResult ) { throw WinApiException("GetExitCodeThread"); }
        if( 0 == dwExitCode ) { throw WinApiException("Remote LoadLibrary returned", GetLastError()); }
    }
    catch( WinApiException & )
    {
        if( NULL != hThread )  { CloseHandle( hThread ); }
        if( NULL != hKernel32Dll ) { FreeLibrary( hKernel32Dll ); }
        if( NULL != lpBaseAddr ) { VirtualFreeEx( hProcess, lpBaseAddr, 0, MEM_RELEASE ); }
        if( NULL != hProcess ) { CloseHandle( hProcess ); }

        throw;
    }

    if( NULL != hThread )  { CloseHandle( hThread ); }
    if( NULL != hKernel32Dll ) { FreeLibrary( hKernel32Dll ); }
    if( NULL != lpBaseAddr ) { VirtualFreeEx( hProcess, lpBaseAddr, 0, MEM_RELEASE ); }
    if( NULL != hProcess ) { CloseHandle( hProcess ); }
}

