#include "stdafx.h"
#include "DebugConsole.h"
#include "Payload.Dll.h"
#include "WinApiException.h"

void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD eventType, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime )
{
    if( OBJID_WINDOW != idObject ) { return; }

    RECT rect;
    DWORD dwProcessId;
    DWORD id = GetWindowThreadProcessId( hwnd, &dwProcessId );

    BOOL success = GetWindowRect( hwnd, &rect );
    cout << 
        "id: " << id << ", " <<
        "success: " << success << ", " <<
        "left: " << rect.left << ", " <<
        "top: " << rect.top << " ||| " <<
        "event: " << hWinEventHook << ", " <<
        "type: " << eventType << ", " <<
        "hwnd: " << hwnd << ", " <<
        "object: " << idObject << ", " <<
        "child: " << idChild << ", " <<
        "thread: " << dwEventThread << ", " <<
        "time: " << dwmsEventTime <<               
        endl;
}

HOOKDLL_API void APIENTRY StartHooking( DWORD pid )
{
    DebugConsole debugConsole;

    // Register window class
    WNDCLASSEX wcex; 
    ZeroMemory( &wcex, sizeof(wcex) );
    wcex.cbSize = sizeof(wcex);
    wcex.lpfnWndProc = DefWindowProc;
    wcex.lpszClassName = TEXT("Tagger_hook_class");
    ATOM atom = RegisterClassEx( &wcex );
    if( 0 == atom ) { throw  WinApiException("RegisterClassEx"); }

    // Create message window
    HINSTANCE instance = GetModuleHandle(NULL);
    if( NULL == instance ) { throw WinApiException("GetModuleHandle"); }

    HWND messageWindow = CreateWindowEx( 0, wcex.lpszClassName, NULL, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, instance, NULL );
    if( NULL == messageWindow ) { throw WinApiException("CreateWindowEx"); }

    // Hook selected process
    HWINEVENTHOOK hook = SetWinEventHook( EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, NULL, WinEventProc, pid, 0, WINEVENT_OUTOFCONTEXT ); 
    if( 0 == hook ) { throw WinApiException("SetWinEventHook"); }

    // Main message loop to process hooked events
    MSG msg;
    while( GetMessage( &msg, NULL, 0, 0 ) ) { DispatchMessage( &msg ); }

    // Unhook when done
    UnhookWinEvent( hook );
}



