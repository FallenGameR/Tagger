#include "stdafx.h"
#include "DebugConsole.h"
#include "Payload.Dll.h"

//#if 0
// Constants
TCHAR szTitle[] = TEXT("Windows Target x86");
TCHAR szWindowClass[] = TEXT("TARGET");

// Global Variables
PROCESS_INFORMATION hTargetConsole;
HWINEVENTHOOK hWinEventHook;
DebugConsole debugConsole;

// Forwards
void Initialization( DWORD processId );
void Cleanup();
void CreateTargetConsoleWindow();
void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime );
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );

DWORD g_pid = 0;

HOOKDLL_API int APIENTRY StartHooking( DWORD pid )
{
    WNDCLASSEX wcex; 
    MSG msg;

    g_pid = pid;

    // Register window class
    ZeroMemory( &wcex, sizeof(WNDCLASSEX) );
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc = WndProc;
    wcex.lpszClassName = szWindowClass;
    RegisterClassEx( &wcex );

    // Initialize message window
    HINSTANCE hInstance = GetModuleHandle(NULL);
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
        CreateTargetConsoleWindow();
        Initialization(g_pid);
        break;

    case WM_DESTROY:
        Cleanup();
        PostQuitMessage( 0 );
        break;
    }

    return DefWindowProc( hWnd, message, wParam, lParam );
}

void CreateTargetConsoleWindow() 
{
    STARTUPINFO startInfo;
    TCHAR programName[] = TEXT("cmd");

    debugConsole.Open();

    ZeroMemory( &startInfo, sizeof(startInfo) );
    startInfo.cb = sizeof(startInfo);
    startInfo.dwX = 400;
    startInfo.dwY = 400;
    startInfo.dwFlags = STARTF_USEPOSITION;

    //BOOL success = CreateProcess( NULL, programName, NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL, NULL, &startInfo, &hTargetConsole );
    //if( FALSE == success ) { throw "CreateProcess"; }
}

void Initialization( DWORD processId )
{
    // How to find Process ID for corresponding conhost.exe?
    // - it is not parent process
    // - if set up global hook than collection of handles to hooked processes needed to be handles - read/write async access to it. Imagine lock for global hook =)

    hWinEventHook = SetWinEventHook( 
        EVENT_OBJECT_LOCATIONCHANGE, 
        EVENT_OBJECT_LOCATIONCHANGE, 
        NULL, 
        WinEventProc,
        processId, 
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

    if( dwProcessId == hTargetConsole.dwProcessId )
    {
        cout << "!!!!" << endl;
    }

    GetWindowRect( hwnd, &rect );
    cout << eventType <<  ", left: " << rect.left << ", top: " << rect.top << endl;
}

//#endif