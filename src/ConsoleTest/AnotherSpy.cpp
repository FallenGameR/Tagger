#include "stdafx.h"
#include "DebugConsole.h"

// Constants
TCHAR szTitle[] = TEXT("Windows Target x86");
TCHAR szWindowClass[] = TEXT("TARGET");

// Global Variables
PROCESS_INFORMATION hTargetConsole;
HWINEVENTHOOK hWinEventHook;
DebugConsole debugConsole;

// Forwards
void Initialization();
void Cleanup();
void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime );
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );

int APIENTRY _tWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
{
    WNDCLASSEX wcex; 
    MSG msg;

    // Register window class
    ZeroMemory( &wcex, sizeof(WNDCLASSEX) );
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc = WndProc;
    wcex.lpszClassName = szWindowClass;
    RegisterClassEx( &wcex );

    // Initialize message window
    HWND hWnd = CreateWindowEx( 0, szWindowClass, szTitle, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, hInstance, NULL );
    if( !hWnd ) { return FALSE; }

    // Main message loop
    UpdateWindow( hWnd );
    while( GetMessage( &msg, NULL, 0, 0 ) ) { DispatchMessage( &msg ); }

    return (int) msg.wParam;
}

LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    switch( message )
    {
    case WM_CREATE:
        Initialization();
        break;

    case WM_DESTROY:
        Cleanup();
        PostQuitMessage( 0 );
        break;
    }

    return DefWindowProc( hWnd, message, wParam, lParam );
}

void Initialization()
{
    STARTUPINFO startInfo;
    TCHAR programName[] = TEXT("cmd");

    debugConsole.Open();

    ZeroMemory( &startInfo, sizeof(startInfo) );
    startInfo.cb = sizeof(startInfo);
    startInfo.dwX = 400;
    startInfo.dwY = 400;
    startInfo.dwFlags = STARTF_USEPOSITION;

    BOOL success = CreateProcess( NULL, programName, NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL, NULL, &startInfo, &hTargetConsole );
    if( FALSE == success ) { throw "CreateProcess"; }

    hWinEventHook = SetWinEventHook( 
        EVENT_OBJECT_LOCATIONCHANGE, 
        EVENT_OBJECT_LOCATIONCHANGE, 
        NULL, 
        WinEventProc, 
        0, 
        0, 
        WINEVENT_OUTOFCONTEXT ); 
    if( !hWinEventHook ) { throw "SetWinEventHook"; }

    cout << "Please start typing to see event information" << endl;
}

void Cleanup()
{
    UnhookWinEvent( hWinEventHook );
    debugConsole.Close();
}

void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD eventType, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime )
{
    RECT rect;
    DWORD dwProcessId;

    GetWindowThreadProcessId( hwnd, &dwProcessId );

    GetWindowRect( hwnd, &rect );
    cout << eventType <<  ", left: " << rect.left << ", top: " << rect.top << endl;
    break;
}

