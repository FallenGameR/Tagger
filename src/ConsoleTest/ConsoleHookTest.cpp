#include "stdafx.h"

// Windows Header Files
#define WIN32_LEAN_AND_MEAN
#include <SDKDDKVer.h>
#include <windows.h>

// C RunTime Header Files
#include <tchar.h>
#include <conio.h>
#include <iostream>
#include <sstream>
#include <Strsafe.h> 

// Using std namespace
using namespace std;

// Constants
#define BUFFSIZE  128
TCHAR* szTitle = TEXT("Windows Target x86");
TCHAR* szWindowClass = TEXT("TARGET");

// Global Variables
PROCESS_INFORMATION hConsole1, hConsole2;
HWINEVENTHOOK   hEventHook;
DWORD g_dwCurrentProc;

// Forwards
void DemoInitialization();
void CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime );
void InstallWinEventsHook();
void UninstallWinEventsHook();
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );

int APIENTRY _tWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
{
    // Register window class
    WNDCLASSEX wcex; 
    ZeroMemory( &wcex, sizeof(WNDCLASSEX) );
    wcex.cbSize         = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc	= WndProc;
    wcex.lpszClassName	= szWindowClass;
    RegisterClassEx( &wcex );

    // Initialize message window
    HWND hWnd = CreateWindowEx( 0, szWindowClass, szTitle, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, hInstance, NULL );
    if( !hWnd ) { return FALSE; }
    UpdateWindow( hWnd );

    // Main message loop
    MSG msg;
    while( GetMessage( &msg, NULL, 0, 0 ) )
    {
        DispatchMessage( &msg );
    }

    return (int) msg.wParam;
}

LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    BOOL hookSuccess;

    switch( message )
    {
    case WM_CREATE:
        DemoInitialization();
        InstallWinEventsHook();
        break;

    case WM_DESTROY:
        UninstallWinEventsHook();
        PostQuitMessage( 0 );
        break;
    }

    return DefWindowProc( hWnd, message, wParam, lParam );
}

void DemoInitialization()
{
    LPTSTR lpstrMyprompt;
    LPTSTR lpstrEditcmd;
    LPTSTR lpstrSysDir;
    DWORD cWritten;
    TCHAR buffer[BUFFSIZE];
    TCHAR buffer2[BUFFSIZE];
    TCHAR buffer3[BUFFSIZE];
    STARTUPINFO hStartUp;
    RECT   rcConsole2;

    // Check for AttachConsole
    memset(&hStartUp, 0, sizeof(hStartUp));   
    hStartUp.cb = sizeof(hStartUp);  
    lpstrMyprompt = &buffer[0];
    lpstrEditcmd = &buffer2[0];
    lpstrSysDir = &buffer3[0];

    GetSystemDirectory(lpstrSysDir,BUFFSIZE);

    size_t cb = sizeof(TCHAR) * BUFFSIZE;
    StringCbPrintf(lpstrEditcmd, cb, L"%s\\cmd.exe", lpstrSysDir);
    StringCbCopy(lpstrMyprompt, cb, L"Console #1");

    // First, we have to create two consoles
    if (FALSE == CreateProcess(lpstrEditcmd, NULL, NULL, NULL, FALSE, CREATE_NEW_CONSOLE | NORMAL_PRIORITY_CLASS, NULL, NULL, &hStartUp, &hConsole1))
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't create a new console: %d.", GetLastError());
        MessageBox(NULL, lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    StringCbPrintf(lpstrEditcmd, cb, L"\nAttached\n"); 

    Sleep(1000);
    BOOL bRet = AttachConsole(hConsole1.dwProcessId);
    if (bRet)
    {
        HANDLE hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );

        if (FALSE == WriteConsole( hStdOut, lpstrEditcmd, lstrlen(lpstrMyprompt), &cWritten, NULL))
        {
            StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't attach to the console: %d.", GetLastError()); 
            MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        }

        SetConsoleTitle (lpstrMyprompt);
        MessageBox (NULL, L"Successfully attached to console #1", L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        HWND hWnd = GetConsoleWindow();
        GetWindowRect(hWnd,&rcConsole2);

        hStartUp.dwX = rcConsole2.left + 100;
        hStartUp.dwY = rcConsole2.top + 100;
        hStartUp.dwXSize = rcConsole2.right - rcConsole2.left;
        hStartUp.dwYSize = rcConsole2.bottom - rcConsole2.top;
        hStartUp.dwFlags = STARTF_USEPOSITION;
        g_dwCurrentProc = hConsole1.dwProcessId;
    }
    else
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't attach to the console: %d.", GetLastError()); 
        MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
    }

    FreeConsole();

    StringCbPrintf(lpstrEditcmd, cb, L"%s\\cmd.exe", lpstrSysDir);
    StringCbCopy(lpstrMyprompt, cb, L"Console #2");

    // First, we have to create two consoles
    if (FALSE == CreateProcess(lpstrEditcmd, NULL, NULL, NULL, FALSE, CREATE_NEW_CONSOLE | NORMAL_PRIORITY_CLASS, NULL, NULL, &hStartUp, &hConsole2))
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't create a new console: %d.", GetLastError()); 
        MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    Sleep(1000);
    bRet = AttachConsole(hConsole2.dwProcessId);
    if (FALSE == bRet)
    {
        StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", GetLastError()); 
        MessageBox(NULL,buffer, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    SetConsoleTitle (lpstrMyprompt);

    FreeConsole();
}

VOID CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime )
{
    DWORD dwProcessId;
    GetWindowThreadProcessId(hwnd,&dwProcessId);

    if (((dwProcessId == hConsole1.dwProcessId) || 
        (dwProcessId == hConsole2.dwProcessId)) &&
        (dwProcessId == g_dwCurrentProc))
    {
        return;
    }

    RECT rect;
    CHAR Buf[BUFFSIZE];
    size_t cb = sizeof(TCHAR) * BUFFSIZE;
    size_t len = 0;
    HANDLE hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );
    DWORD cWritten;

    std::stringstream ss;

    switch (event)
    {
    //case EVENT_SYSTEM_MOVESIZESTART:
    //case EVENT_SYSTEM_MOVESIZEEND:
    case EVENT_OBJECT_LOCATIONCHANGE:
        {
            GetWindowRect( hwnd, &rect );

            ss << event <<  ", left: " << rect.left << ", top: " << rect.top << endl;

            StringCbPrintfA(Buf, cb, ss.str().c_str(), idObject, idChild );
            StringCbLengthA(Buf, cb, &len);                     

            if (FALSE == WriteConsoleA( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintfA(Buf, cb, "Error, couldn't write to the console: %d.", GetLastError());
                MessageBoxA(NULL, Buf, "Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
            break;
        }
    }
}

void InstallWinEventsHook()
{
    // Set up event call back
    // We want all events
    // Use our own module
    // All processes
    // All threads
    hEventHook = SetWinEventHook( EVENT_MIN, EVENT_MAX, NULL, WinEventProc, 0, 0, WINEVENT_OUTOFCONTEXT); 
    if( !hEventHook ) { throw "SetWinEventHook"; }

    BOOL bRet = AttachConsole(hConsole1.dwProcessId);
    if( !bRet ) { throw "AttachConsole"; }

    cprintf( "Please start typing to see event information\n" );
    cout << "Please start typing to see event information" << endl;
    g_dwCurrentProc = hConsole1.dwProcessId;
}

void UninstallWinEventsHook()
{
    UnhookWinEvent(hEventHook);
}
