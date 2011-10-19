#include "stdafx.h"

// Track and Find Consoles
// http://msdn.microsoft.com/en-us/library/ms971319.aspx#console

// How To Determine if the Current Application is a Console
// Use AttachConsole to determine if the current window is a console.

// Console Specific Events
//
// The console triggers several WinEvents to provide more comprehensive information about the interactions between the user and console applications. Following are scenarios for each event:
//
// EVENT_CONSOLE_CARET is triggered, inform the user that the caret position has changed and whether the caret is selecting text or is visible.
// EVENT_CONSOLE_UPDATE_SIMPLE is triggered, inform the user that this event was received and then display the character that was just entered.
// EVENT_CONSOLE_LAYOUT is triggered, inform the user that this event was received.
// EVENT_CONSOLE_START_ APPLICATION is triggered, inform the user that this event was received. This event determines when an application is started from within a DOS box.
// EVENT_CONSOLE_END_ APPLICATION is triggered, inform the user that this event was received.
// EVENT_OBJECT_CREATE is triggered, track the event to determine when a console is created.
// EVENT_OBJECT_FOCUS is triggered, you can determine when a console has focus or not.


#define BUFFSIZE  128

PROCESS_INFORMATION hConsole1, hTargetConsole;
typedef BOOL (WINAPI* PFN_AttachConsole)(DWORD);
typedef HWND (WINAPI* PFN_GetConsoleWindow)(VOID);

PFN_AttachConsole fpAttachConsole;
PFN_GetConsoleWindow fpGetConsoleWindow;
DWORD g_dwCurrentProc;
void createConsoles(void);

// Main entry point (the function arguments are ignored).
int _tmain1(int argc, _TCHAR* argv[])
{
    createConsoles();
    return 0;
}

void createConsoles(void)
{
    int iResponse;
    iResponse = IDOK;
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
    StringCbPrintf(lpstrEditcmd, cb, L"%s\\edit.com", lpstrSysDir);
    StringCbCopy(lpstrMyprompt, cb, L"Console #1");

    // First, we have to create two consoles
    if (FALSE == CreateProcess(lpstrEditcmd, NULL, NULL, NULL, FALSE, CREATE_NEW_CONSOLE | NORMAL_PRIORITY_CLASS, NULL, NULL, &hStartUp, &hConsole1))
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't create a new console: %d.", GetLastError());
        MessageBox(NULL, lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    StringCbPrintf(lpstrEditcmd, cb, L"\nAttached\n"); 

    Sleep(5000);
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

        hStartUp.dwX = rcConsole2.left + 10;
        hStartUp.dwY = rcConsole2.bottom + 10;
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

    StringCbPrintf(lpstrEditcmd, cb, L"%s\\edit.com", lpstrSysDir);
    StringCbCopy(lpstrMyprompt, cb, L"Console #2");

    // First, we have to create two consoles
    if (FALSE == CreateProcess(lpstrEditcmd, NULL, NULL, NULL, FALSE, CREATE_NEW_CONSOLE | NORMAL_PRIORITY_CLASS, NULL, NULL, &hStartUp, &hTargetConsole))
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't create a new console: %d.", GetLastError()); 
        MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    Sleep(5000);
    bRet = fpAttachConsole(hTargetConsole.dwProcessId);
    if (FALSE == bRet)
    {
        StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", GetLastError()); 
        MessageBox(NULL,buffer, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        return;
    }

    SetConsoleTitle (lpstrMyprompt);

    FreeConsole();

}

HWINEVENTHOOK   hWinEventHook;

#define  EVENT_CONSOLE_CARET              0x4001
#define  EVENT_CONSOLE_UPDATE_REGION      0x4002
#define  EVENT_CONSOLE_UPDATE_SIMPLE      0x4003
#define  EVENT_CONSOLE_UPDATE_SCROLL      0x4004
#define  EVENT_CONSOLE_LAYOUT             0x4005
#define  EVENT_CONSOLE_START_APPLICATION  0x4006
#define  EVENT_CONSOLE_END_APPLICATION    0x4007

#define WINEVENTDLL_API __declspec(dllexport)

/*******************************************************************
WinEventProc() - Callback function handles events
*******************************************************************/
WINEVENTDLL_API VOID CALLBACK WinEventProc( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime )
{

    DWORD dwProcessId;
    GetWindowThreadProcessId(hwnd,&dwProcessId);

    if (((dwProcessId == hConsole1.dwProcessId) || 
        (dwProcessId == hTargetConsole.dwProcessId)) &&
        (dwProcessId == g_dwCurrentProc))
        return;

    TCHAR Buf[BUFFSIZE];
    size_t cb = sizeof(TCHAR) * BUFFSIZE;
    size_t len = 0;
    HANDLE hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );
    DWORD cWritten;

    switch (event)
    {

    case EVENT_CONSOLE_CARET:
        {

            if( 1 == idObject )      //CONSOLE_CARET_SELECTION
                StringCbCopy(Buf, cb, L"idObject: CONSOLE CARET SELECTION - ");
            else if( 2 == idObject ) //CONSOLE_CARET_VISIBLE
                StringCbCopy(Buf, cb, L"idObject: CONSOLE CARET VISIBLE - ");
            else
                StringCbPrintf(Buf, cb, L" idObject: %d - INVALID VALUE!! - ", idObject );

            StringCbPrintf(Buf, cb, L"\r\nEvent Console CARET!\r\nX: \ %d - Y: %d \r\n\r\n", LOWORD(idChild), HIWORD(idChild) );
            StringCbLength(Buf, cb, &len); 
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
        }
        break;


    case EVENT_CONSOLE_UPDATE_REGION:
        {
            StringCbPrintf(Buf, cb, L"Event Console UPDATE REGION!\r\n \ Left: %d - Top: %d - Right: %d - Bottom:%d \r\n\r\n", LOWORD(idObject), HIWORD(idObject), LOWORD(idChild), HIWORD(idChild) );
            StringCbLength(Buf, cb, &len); 
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL,Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;


    case EVENT_CONSOLE_UPDATE_SIMPLE:
        {
            StringCbPrintf(Buf, cb, L"\r\nEvent Console UPDATE SIMPLE!\r\n \ X: %d - Y: %d\t Char: %d Attr: %d\r\n\r\n", LOWORD(idObject), HIWORD(idObject), LOWORD(idChild), HIWORD(idChild) );
            StringCbLength(Buf, cb, &len); 
            if (FALSE == WriteConsole( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;


    case EVENT_CONSOLE_UPDATE_SCROLL:
        {
            StringCbPrintf(Buf, cb, L"Event Console UPDATE SCROLL!\r\ndx: \ %d  -  dy: %d\r\n\r\n", idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
        }
        break;


    case EVENT_CONSOLE_LAYOUT:
        {
            StringCbPrintf(Buf, cb, L"Event Console LAYOUT!\r\n", idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;


    case EVENT_CONSOLE_START_APPLICATION:
        {
            StringCbPrintf(Buf, cb, L"Event Console START APPLICATION!\r\n \ Process ID: %d - Child ID: %d\r\n\r\n", idObject, idChild ); StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
        }
        break;


    case EVENT_CONSOLE_END_APPLICATION:
        {
            StringCbPrintf(Buf, cb, L"Event Console END APPLICATION!\r\n \ Process ID: %d - Child ID: %d\r\n\r\n", idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL ))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;
    }

    return;

}


/*************************************************************************
InstallWinEventsHook() - Initalize the Active Accessibility subsystem
*************************************************************************/
WINEVENTDLL_API BOOL InstallWinEventsHook()
{

    TCHAR buffer[BUFFSIZE];
    LPTSTR lpstrMyprompt;
    size_t cb = sizeof(TCHAR) * BUFFSIZE;
    size_t len = 0;

    lpstrMyprompt = &buffer[0];

    // Set up event call back
    hWinEventHook = SetWinEventHook(EVENT_CONSOLE_CARET,               
        // We want all events
        EVENT_CONSOLE_END_APPLICATION,     
        NULL,         // Use our own module
        WinEventProc, // Our callback function
        0,            // All processes
        0,            // All threads
        WINEVENT_SKIPOWNPROCESS | 
        WINEVENT_OUTOFCONTEXT); 

    // Did we install correctly? 
    if (hWinEventHook)
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

/*************************************************************************
UninstallWinEventsHook() - Shuts down the Active Accessibility subsystem
*************************************************************************/
WINEVENTDLL_API BOOL Cleanup(void)
{

    // Remove the WinEvent hook    
    UnhookWinEvent(hWinEventHook);

    return(TRUE);
}
