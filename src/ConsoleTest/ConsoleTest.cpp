// Track and Find Consoles
#include "stdafx.h"
#include <stdio.h>
#include <tchar.h>
#include <windows.h>
#include <Strsafe.h> 

#define BUFFSIZE  128

PROCESS_INFORMATION hConsole1, hConsole2;
typedef BOOL (WINAPI* PFN_AttachConsole)(DWORD);
typedef HWND (WINAPI* PFN_GetConsoleWindow)(VOID);

PFN_AttachConsole fpAttachConsole;
PFN_GetConsoleWindow fpGetConsoleWindow;
DWORD g_dwCurrentProc;
void createConsoles(void);

// Main entry point (the function arguments are ignored).
int _tmain(int argc, _TCHAR* argv[])
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
    if (FALSE == CreateProcess(lpstrEditcmd, NULL, NULL, NULL, FALSE,
        CREATE_NEW_CONSOLE | NORMAL_PRIORITY_CLASS, NULL, 
        NULL, &hStartUp, &hConsole1))
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't create a new console: %d.", 
            GetLastError());
        MessageBox(NULL, lpstrMyprompt, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);

        return;
    }

    StringCbPrintf(lpstrEditcmd, cb, L"\nAttached\n"); 

    Sleep(5000);
    BOOL bRet = fpAttachConsole(hConsole1.dwProcessId);
    if (bRet)
    {
        HANDLE hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );

        if (FALSE == WriteConsole( hStdOut, lpstrEditcmd, lstrlen(lpstrMyprompt),
            &cWritten, NULL))
        {
            StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't attach to the console: %d.", 
                GetLastError()); 
            MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
        }

        SetConsoleTitle (lpstrMyprompt);
        MessageBox (NULL, L"Successfully attached to console #1", 
            L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
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
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't attach to the console: %d.", 
            GetLastError()); 
        MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
    }

    FreeConsole();

    StringCbPrintf(lpstrEditcmd, cb, L"%s\\edit.com", lpstrSysDir);
    StringCbCopy(lpstrMyprompt, cb, L"Console #2");

    // First, we have to create two consoles
    if (FALSE == CreateProcess(lpstrEditcmd, NULL, NULL, NULL, FALSE,
        CREATE_NEW_CONSOLE | NORMAL_PRIORITY_CLASS,
        NULL, NULL, &hStartUp, &hConsole2))
    {
        StringCbPrintf(lpstrMyprompt, cb, L"Error, couldn't create a new console: %d.", 
            GetLastError()); 
        MessageBox(NULL,lpstrMyprompt, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
        return;
    }

    Sleep(5000);
    bRet = fpAttachConsole(hConsole2.dwProcessId);
    if (FALSE == bRet)
    {
        StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", 
            GetLastError()); 
        MessageBox(NULL,buffer, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
        return;
    }

    SetConsoleTitle (lpstrMyprompt);

    FreeConsole();

}


//-----------------------------------------------------------------------------------------


// Extracting Console Information

void DisplayRegion()
{
    int i;
    HANDLE hStdout;
    LPTSTR lpstrMyprompt;
    DWORD cWritten;
    TCHAR buffer[BUFFSIZE];
    lpstrMyprompt = &buffer[0];
    size_t cb = sizeof(TCHAR) * BUFFSIZE;

    CHAR_INFO chiBuffer[3200];
    COORD coordBufSize, coordBufCoord;
    SMALL_RECT srctReadRect, srctWriteRect;

    FreeConsole();

    BOOL bRet = fpAttachConsole(hConsole1.dwProcessId);
    if (FALSE == bRet)
    {
        StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", 
            GetLastError());
        MessageBox(NULL, buffer, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
        return;
    }

    hStdout = GetStdHandle(STD_OUTPUT_HANDLE); 

    for (i = 1; i < 10; i++)
    {

        StringCbPrintf(buffer, cb, L"\nLine %d to copy over...\n", i);
        WriteConsole(
            hStdout,
            buffer,
            lstrlen(buffer),
            &cWritten,
            NULL
            );
    }
    srctReadRect.Top=0;
    srctReadRect.Left=0;
    srctReadRect.Bottom=50;
    srctReadRect.Right=40;

    coordBufSize.X=40;
    coordBufSize.Y=50;

    coordBufCoord.X=0;
    coordBufCoord.Y=0;

    ReadConsoleOutput(
        hStdout,
        chiBuffer,
        coordBufSize,
        coordBufCoord,
        &srctReadRect
        );

    srctWriteRect.Left=0;
    srctWriteRect.Top=0;
    srctWriteRect.Right=40;
    srctWriteRect.Bottom=50;

    FreeConsole();

    bRet = fpAttachConsole(hConsole2.dwProcessId);
    if (FALSE == bRet)
    {
        StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", 
            GetLastError());
        MessageBox(NULL,buffer, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
        return;
    }

    hStdout = GetStdHandle(STD_OUTPUT_HANDLE); 

    WriteConsoleOutput(
        hStdout,
        chiBuffer,
        coordBufSize,
        coordBufCoord,
        &srctWriteRect
        );

    FreeConsole();

    return;

}



//--------------------------------------------------------------------------------------------


// Extracting Console Information

void CopyDisplayedText(void) 
{
   int i;
   HANDLE hStdout;
   DWORD cWritten;
   TCHAR buffer[BUFFSIZE];
   size_t cb = sizeof(TCHAR) * BUFFSIZE;

   CHAR_INFO chiBuffer[3200];
   COORD coordBufSize, coordBufCoord;
   SMALL_RECT srctReadRect, srctWriteRect;
   CONSOLE_SCREEN_BUFFER_INFO lConsoleScreenBufferInfo;

   FreeConsole();
   
   BOOL bRet = fpAttachConsole(hConsole1.dwProcessId);
   if (FALSE == bRet)
   {
      StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", 
          GetLastError());
      MessageBox(NULL,buffer, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
      return;
   }
   
   hStdout = GetStdHandle(STD_OUTPUT_HANDLE); 

   for (i = 1; i < 30; i++)
   {
      StringCbPrintf(buffer, cb, L"CopyDisplayedText: Line to ignore %d\n", i);
      WriteConsole(
         hStdout,
         buffer,
         lstrlen(buffer),
         &cWritten,
         NULL
         );
   }

   StringCbPrintf(buffer, cb, L"\n\n");
   for (i = 1; i < 10; i++)
   {
      WriteConsole(
         hStdout,
         buffer,
         lstrlen(buffer),
         &cWritten,
         NULL
         );
   }


   for (i = 1; i < 20; i++)
   {
      StringCbPrintf(buffer, cb, L"CopyDisplayedText:Copy Line %d\n", i);
      WriteConsole(
         hStdout,
         buffer,
         lstrlen(buffer),
         &cWritten,
         NULL
         );
   }

   MessageBox (NULL, L"Copy to new window", L"APIs", MB_OK);

   // Get the screen buffer information and set the rectangles
   GetConsoleScreenBufferInfo(
      hStdout, &lConsoleScreenBufferInfo
      );
   srctReadRect.Top=lConsoleScreenBufferInfo.srWindow.Top;
   srctReadRect.Left=lConsoleScreenBufferInfo.srWindow.Left;
   srctReadRect.Bottom=lConsoleScreenBufferInfo.srWindow.Bottom;
   srctReadRect.Right = lConsoleScreenBufferInfo.srWindow.Right;

   coordBufSize.X=lConsoleScreenBufferInfo.srWindow.Right;
   coordBufSize.Y=lConsoleScreenBufferInfo.srWindow.Bottom;
   coordBufCoord.X=lConsoleScreenBufferInfo.srWindow.Left;
   coordBufCoord.Y=lConsoleScreenBufferInfo.srWindow.Top;

   // Read the window
   ReadConsoleOutput(
      hStdout,
      chiBuffer,
      coordBufSize,
      coordBufCoord,
      &srctReadRect
      );

   srctWriteRect.Left=lConsoleScreenBufferInfo.srWindow.Left;
   srctWriteRect.Top=lConsoleScreenBufferInfo.srWindow.Top;
   srctWriteRect.Right=lConsoleScreenBufferInfo.srWindow.Right;
   srctWriteRect.Bottom=lConsoleScreenBufferInfo.srWindow.Bottom;

   FreeConsole();

   bRet = fpAttachConsole(hConsole2.dwProcessId);
   if (FALSE == bRet)
   {
      StringCbPrintf(buffer, cb, L"Error, couldn't attach to the console: %d.", 
          GetLastError());
      MessageBox(NULL,buffer, L"Track and Find Consoles", 
            MB_OK | MB_SYSTEMMODAL);
      return;
   }

   hStdout = GetStdHandle(STD_OUTPUT_HANDLE); 
   
   // Display the buffer in the new window
   WriteConsoleOutput(
      hStdout,
      chiBuffer,
      coordBufSize,
      coordBufCoord,
      &srctWriteRect
      );
   FreeConsole();

   MessageBox (NULL, L"Return to original console.",
       L"APIs", MB_OK);
return;
}


//-------------------------------------------------------------------------------------------------------------



HWINEVENTHOOK   hEventHook;

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
WINEVENTDLL_API VOID CALLBACK WinEventProc( HWINEVENTHOOK 
    hWinEventHook, DWORD event, HWND hwnd, 
    LONG idObject, LONG idChild, 
    DWORD dwEventThread, DWORD dwmsEventTime )
{

    DWORD dwProcessId;
    GetWindowThreadProcessId(hwnd,&dwProcessId);

    if (((dwProcessId == hConsole1.dwProcessId) || 
        (dwProcessId == hConsole2.dwProcessId)) &&
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
                StringCbPrintf(Buf, cb, L" idObject: %d - INVALID VALUE!! - ", 
                idObject );

            StringCbPrintf(Buf, cb, L"\r\nEvent Console CARET!\r\nX: \
                                     %d - Y: %d \r\n\r\n", 
                                     LOWORD(idChild), HIWORD(idChild) );
            StringCbLength(Buf, cb, &len); 
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
        }
        break;


    case EVENT_CONSOLE_UPDATE_REGION:
        {
            StringCbPrintf(Buf, cb, L"Event Console UPDATE REGION!\r\n \
                                     Left: %d - Top: %d - Right: %d - Bottom:%d \r\n\r\n", 
                                     LOWORD(idObject), HIWORD(idObject), LOWORD(idChild), 
                                     HIWORD(idChild) );
            StringCbLength(Buf, cb, &len); 
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
                MessageBox(NULL,Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;


    case EVENT_CONSOLE_UPDATE_SIMPLE:
        {
            StringCbPrintf(Buf, cb, L"\r\nEvent Console UPDATE SIMPLE!\r\n \
                                     X: %d - Y: %d\t Char: %d Attr: %d\r\n\r\n", 
                                     LOWORD(idObject), HIWORD(idObject), 
                                     LOWORD(idChild), HIWORD(idChild) );
            StringCbLength(Buf, cb, &len); 
            if (FALSE == WriteConsole( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;


    case EVENT_CONSOLE_UPDATE_SCROLL:
        {
            StringCbPrintf(Buf, cb, L"Event Console UPDATE SCROLL!\r\ndx: \
                                     %d  -  dy: %d\r\n\r\n", idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole( hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
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
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

        }
        break;


    case EVENT_CONSOLE_START_APPLICATION:
        {
            StringCbPrintf(Buf, cb, L"Event Console START APPLICATION!\r\n \
                                     Process ID: %d - Child ID: %d\r\n\r\n", 
                                     idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
                MessageBox(NULL, Buf, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }
        }
        break;


    case EVENT_CONSOLE_END_APPLICATION:
        {
            StringCbPrintf(Buf, cb, L"Event Console END APPLICATION!\r\n \
                                     Process ID: %d - Child ID: %d\r\n\r\n", 
                                     idObject, idChild );
            StringCbLength(Buf, cb, &len);                     
            if (FALSE == WriteConsole(hStdOut, Buf, len, &cWritten, NULL ))
            {
                StringCbPrintf(Buf, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
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
    hEventHook = SetWinEventHook(EVENT_CONSOLE_CARET,               
        // We want all events
        EVENT_CONSOLE_END_APPLICATION,     
        NULL,         // Use our own module
        WinEventProc, // Our callback function
        0,            // All processes
        0,            // All threads
        WINEVENT_SKIPOWNPROCESS | 
        WINEVENT_OUTOFCONTEXT); 

    // Did we install correctly? 
    if (hEventHook)
    {
        BOOL bRet = fpAttachConsole(hConsole1.dwProcessId);
        if (bRet)
        {
            HANDLE hStdout = GetStdHandle( STD_OUTPUT_HANDLE );
            DWORD cWritten;

            StringCbPrintf(buffer, cb, 
                L"\nPlease start typing to see event information\n");
            StringCbLength(buffer, cb, &len);                     
            if (FALSE == WriteConsole(hStdout, buffer, len, &cWritten, NULL))
            {
                StringCbPrintf(buffer, cb, L"Error, couldn't write to the console: %d.", 
                    GetLastError());
                MessageBox(NULL, buffer, L"Track and Find Consoles", MB_OK | MB_SYSTEMMODAL);
            }

            g_dwCurrentProc = hConsole1.dwProcessId;
            return TRUE;
        }
    }

    // Did not install properly - fail
    StringCbPrintf(buffer, cb, L"Error, couldn't set windows hook: %d.", 
        GetLastError());
    MessageBox(NULL, lpstrMyprompt, L"Track and Find Consoles", 
        MB_OK | MB_SYSTEMMODAL);
    return FALSE;
}

/*************************************************************************
UninstallWinEventsHook() - Shuts down the Active Accessibility subsystem
*************************************************************************/
WINEVENTDLL_API BOOL UninstallWinEventsHook(void)
{

    // Remove the WinEvent hook    
    UnhookWinEvent(hEventHook);

    return(TRUE);
}
