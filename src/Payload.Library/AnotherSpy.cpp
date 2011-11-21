#include "stdafx.h"
#include "DebugConsole.h"
#include "Payload.Dll.h"

//#if 0
// Constants
TCHAR szTitle[] = TEXT("Windows Target x86");
TCHAR szWindowClass[] = TEXT("TARGET");

// Global Variables
HWINEVENTHOOK hWinEventHook;

// Forwards
void Initialization( DWORD processId );
void Cleanup();
void CreateTargetConsoleWindow();
void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime );
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );

HOOKDLL_API void APIENTRY StartHooking( DWORD pid )
{
    WNDCLASSEX wcex; 
    DebugConsole debugConsole;

    // Register window class
    ZeroMemory( &wcex, sizeof(WNDCLASSEX) );
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc = DefWindowProc;
    wcex.lpszClassName = szWindowClass;
    ATOM atom = RegisterClassEx( &wcex );
    if( 0 == atom ) { throw "RegisterClassEx"; }

    // Initialize message window
    HINSTANCE hInstance = GetModuleHandle(NULL);
    HWND messageWindow = CreateWindowEx( 0, szWindowClass, szTitle, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, hInstance, NULL );
    if( !messageWindow ) { return; }

    hWinEventHook = SetWinEventHook( 
        EVENT_OBJECT_LOCATIONCHANGE, 
        EVENT_OBJECT_LOCATIONCHANGE, 
        NULL, 
        WinEventProc,
        pid, 
        0,
        WINEVENT_OUTOFCONTEXT ); 
    if( !hWinEventHook ) { throw "SetWinEventHook"; }

    // Main message loop
    MSG msg;
    while( GetMessage( &msg, NULL, 0, 0 ) ) { DispatchMessage( &msg ); }

    UnhookWinEvent( hWinEventHook );
}

void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD eventType, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime )
{
    RECT rect;
    DWORD dwProcessId;
    GetWindowThreadProcessId( hwnd, &dwProcessId );

    GetWindowRect( hwnd, &rect );
    cout << eventType <<  ", left: " << rect.left << ", top: " << rect.top << endl;
}

