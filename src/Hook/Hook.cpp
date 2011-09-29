#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

// Including SDKDDKVer.h defines the highest available Windows platform.
#include <SDKDDKVer.h>

// Windows Header Files:
#include <windows.h>

// Project files:
#include "Hook.h"


struct error
{
    char* Message;
    DWORD ErrorCode;

    error( char* message ): Message(message), ErrorCode(GetLastError()) {}
    error( char* message, DWORD lastError): Message(message), ErrorCode(lastError) {}
};


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


int HookMessageQueue( HWND handle, LPSTR outString )
{	
    shared_wnd_handle = handle;

    DWORD processThreadId = GetWindowThreadProcessId( handle, NULL );

    shared_hook = SetWindowsHookEx( WH_CALLWNDPROC, (HOOKPROC) MyHookProcedure, hDll, processThreadId );
    if( NULL == shared_hook ) { throw error("SetWindowsHookEx"); }
    
    ULONG originalWindowProcedure = GetWindowLong( handle, GWL_WNDPROC );
    if( 0 == originalWindowProcedure ) { throw error("GetWindowLong"); }
   
    WM_USERMESSAGE = RegisterWindowMessage( TEXT("WM_USERMESSAGE") );
    if( 0 == WM_USERMESSAGE ) { throw error("RegisterWindowMessage"); }

    SendMessage( handle, WM_USERMESSAGE, 0, 0 );
    //SendMessage( handle, WM_CLOSE, 0, 0 );

    if( 42 != g_test ) { throw error("MyHookProcedure didn't executed"); }

    strcpy( outString, shared_string );
    return strlen(outString);
}
