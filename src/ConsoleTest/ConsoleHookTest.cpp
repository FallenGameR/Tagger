#include "stdafx.h"

// Windows Header Files
#define WIN32_LEAN_AND_MEAN
#include <SDKDDKVer.h>
#include <windows.h>

// C RunTime Header Files
#include <tchar.h>

// Using std namespace
using namespace std;

// Global Variables
TCHAR* szTitle = TEXT("Windows Target x86");
TCHAR* szWindowClass = TEXT("TARGET");
HFONT defaultFont;
RECT currentRect, moveRect, movingRect, sizeRect, sizingRect;

#include <Strsafe.h> 

#define BUFFSIZE  128

PROCESS_INFORMATION hConsole1, hConsole2;
typedef BOOL (WINAPI* PFN_AttachConsole)(DWORD);
typedef HWND (WINAPI* PFN_GetConsoleWindow)(VOID);

HWINEVENTHOOK   hEventHook;
#define WINEVENTDLL_API __declspec(dllexport)

PFN_AttachConsole fpAttachConsole;
PFN_GetConsoleWindow fpGetConsoleWindow;
DWORD g_dwCurrentProc;
void DemoInitialization();
WINEVENTDLL_API VOID CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime );
WINEVENTDLL_API BOOL InstallWinEventsHook();
WINEVENTDLL_API BOOL UninstallWinEventsHook();

BOOL InstallWinEventsHook();


// Forwards
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );

int APIENTRY _tWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
{
    // Register window class
    WNDCLASSEX wcex; ZeroMemory( &wcex, sizeof(WNDCLASSEX) );
    wcex.cbSize         = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc	= WndProc;
    wcex.hInstance		= hInstance;
    wcex.lpszClassName	= szWindowClass;
    RegisterClassEx( &wcex );

    // Initialize message window
    HWND hWnd = CreateWindow( szWindowClass, szTitle, 0, 0, 0, 0, 0, NULL, NULL, hInstance, NULL );
    if( !hWnd ) { return FALSE; }
    SetParent( hWnd, HWND_MESSAGE );
    UpdateWindow( hWnd );

    // Main message loop
    MSG msg;
    while( GetMessage( &msg, NULL, 0, 0 ) )
    {
        TranslateMessage( &msg );
        DispatchMessage( &msg );
    }

    return (int) msg.wParam;
}


LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    TCHAR text[ 4096 ];
    size_t textLength;
    PAINTSTRUCT paint;
    HDC hdc;  
    BOOL hookSuccess;

    switch( message )
    {
    case WM_CREATE:
        DemoInitialization();
        hookSuccess = InstallWinEventsHook();
        if( !hookSuccess ) { throw "InstallWinEventsHook"; }
        break;

    case WM_DESTROY:
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
    HINSTANCE hInst;
    STARTUPINFO hStartUp;
    RECT   rcConsole2;

    // Check for AttachConsole
    hInst = ::LoadLibrary(L"kernel32.dll");
    fpAttachConsole = (BOOL (_stdcall*)(DWORD))GetProcAddress(hInst,"AttachConsole");
    fpGetConsoleWindow = (HWND (_stdcall *)(VOID))GetProcAddress(hInst,"GetConsoleWindow");

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
    BOOL bRet = fpAttachConsole(hConsole1.dwProcessId);
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
        HWND hWnd = fpGetConsoleWindow();
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
    bRet = fpAttachConsole(hConsole2.dwProcessId);
    if (FALSE == bRet)
    {
        StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", GetLastError()); 
        MessageBox(NULL,buffer, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    SetConsoleTitle (lpstrMyprompt);

    FreeConsole();
}

WINEVENTDLL_API VOID CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime )
{
    DWORD dwProcessId;
    GetWindowThreadProcessId(hwnd,&dwProcessId);

    if (((dwProcessId == hConsole1.dwProcessId) || 
        (dwProcessId == hConsole2.dwProcessId)) &&
        (dwProcessId == g_dwCurrentProc))
    {
        return;
    }

    TCHAR Buf[BUFFSIZE];
    size_t cb = sizeof(TCHAR) * BUFFSIZE;
    size_t len = 0;
    HANDLE hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );
    DWORD cWritten;

    switch (event)
    {
    case EVENT_SYSTEM_MOVESIZESTART:
    case EVENT_SYSTEM_MOVESIZEEND:
        {
            StringCbPrintf(Buf, cb, L"Event Console Move!\r\n", idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
            break;
        }
    }
    return;

}

WINEVENTDLL_API BOOL InstallWinEventsHook()
{
    TCHAR buffer[BUFFSIZE];
    LPTSTR lpstrMyprompt;
    size_t cb = sizeof(TCHAR) * BUFFSIZE;
    size_t len = 0;

    lpstrMyprompt = &buffer[0];

    // Set up event call back
    hEventHook = SetWinEventHook(EVENT_MIN,
        // We want all events
        EVENT_MAX,
        NULL,         // Use our own module
        WinEventProc, // Our callback function
        0,            // All processes
        0,            // All threads
        WINEVENT_OUTOFCONTEXT); 

    // Did we install correctly? 
    if (hEventHook)
    {
        BOOL bRet = fpAttachConsole(hConsole1.dwProcessId);
        if (bRet)
        {
            HANDLE hStdout = GetStdHandle( STD_OUTPUT_HANDLE );
            DWORD cWritten;

            StringCbPrintf(buffer, cb, L"\nPlease start typing to see event information\n");
            StringCbLength(buffer, cb, &len);                     
            if (FALSE == WriteConsole(hStdout, buffer, len, &cWritten, NULL))
            {
                StringCbPrintf(buffer, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, buffer, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

            g_dwCurrentProc = hConsole1.dwProcessId;
            return TRUE;
        }
    }

    // Did not install properly - fail
    StringCbPrintf(buffer, cb, L"Error, couldn't set windows hook: %d.", GetLastError());
    MessageBox(NULL, lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
    return FALSE;
}

WINEVENTDLL_API BOOL UninstallWinEventsHook(void)
{
    UnhookWinEvent(hEventHook);
    return TRUE;
}
