/*//////////////////////////////////////////////////////////////////////////////
// MSDN Magazine Bugslayer Column
// Copyright © 2007 John Robbins -- All rights reserved.
//////////////////////////////////////////////////////////////////////////////*/
#include "stdafx.h"

/*//////////////////////////////////////////////////////////////////////////////
// Typedefs
//////////////////////////////////////////////////////////////////////////////*/
// The options structure.
typedef struct tag_OPTIONS
{
    // Show some help action.
    BOOL bShowHelp ; 
    // Do your standard critical section deadlock.
    BOOL bCriticalSection ;
    // Do a mutex deadlock.
    BOOL bMutex ;
    // Wait on one thread at a time with mutexes.
    BOOL bMutexSingleThread ;
    // Block on a critical section and a SendMessage.
    BOOL bCriticalSectionSendMessage ;
    // Block on a process.
    BOOL bProcess ;
} OPTIONS , *POPTIONS ;

typedef unsigned ( __stdcall *PSTARTADDR)( void * ) ;

/*//////////////////////////////////////////////////////////////////////////////
// Function Declarations 
//////////////////////////////////////////////////////////////////////////////*/
BOOL ResolveCommandLine ( OPTIONS & stOpts , int argc , TCHAR * argv[] ) ;
void ShowUsage ( void ) ;
void CriticalSectionDeadlock ( ) ;
void MutexDeadlock ( ) ;
void MutexDeadlockWaitForSingleObject  ( ) ;
void SendMessageAndCriticalSectionDeadlock ( ) ;
void ProcessDeadlock ( ) ;

/*//////////////////////////////////////////////////////////////////////////////
// Externally Visible Code
//////////////////////////////////////////////////////////////////////////////*/

int _tmain ( int argc , _TCHAR* argv[] )
{
    int iReturnValue = 1 ;
    OPTIONS stOpts ;
    BOOL bGoodOptions = ResolveCommandLine ( stOpts , argc , argv ) ; 
    if ( TRUE == bGoodOptions )
    {
        _tprintf ( _T( "Process Id : %d\n" ) , GetCurrentProcessId ( ) ) ;
        // Do the appropriate deadlock.
        if ( TRUE == stOpts.bCriticalSection )
        {
            CriticalSectionDeadlock ( ) ;
        }
        else if ( TRUE == stOpts.bMutex )
        {
            MutexDeadlock ( ) ;
        }
        else if ( TRUE == stOpts.bMutexSingleThread ) 
        {
            MutexDeadlockWaitForSingleObject ( ) ;
        }
        else if ( TRUE == stOpts.bCriticalSectionSendMessage )
        {
            SendMessageAndCriticalSectionDeadlock ( ) ;
        }
        else if ( TRUE == stOpts.bProcess ) 
        {
            ProcessDeadlock ( ) ;
        }
        iReturnValue = 0 ;
    }
    else
    {
        ShowUsage ( ) ;
        iReturnValue = 1 ;
    }
    return ( iReturnValue ) ;
}

/*//////////////////////////////////////////////////////////////////////////////
// File Scope Code     
//////////////////////////////////////////////////////////////////////////////*/

// The common thread spinner

void ThreadSpinner ( PSTARTADDR func1 , PSTARTADDR func2 )
{
    DWORD dwThreadId ;
    HANDLE hThreads[2] ;

    hThreads[0] = (HANDLE) _beginthreadex ( NULL                   ,
                                            0                      ,
                                            (PSTARTADDR)func1      ,
                                            (LPVOID)0              ,
                                            0                      ,
                                            (unsigned*)&dwThreadId  ) ;

    hThreads[1] = (HANDLE)_beginthreadex ( NULL                   ,
                                           0                      ,
                                           (PSTARTADDR)func2      ,
                                           (LPVOID)0              ,
                                           0                      ,
                                           (unsigned*)&dwThreadId  ) ;
    _tprintf (  _T ( "Main thread waiting for background threads to end...\n"));
    WaitForMultipleObjectsEx ( 2 , hThreads , TRUE , INFINITE , FALSE ) ;
}

////////////////////////////////////////////////////////////////////////////////
// CriticalSection Deadlock Start
////////////////////////////////////////////////////////////////////////////////
   
static CRITICAL_SECTION g_csA ;
static CRITICAL_SECTION g_csB ;

DWORD __stdcall CriticalSectionThreadOne ( DWORD )
{
    _tprintf ( _T ( "CriticalSectionThreadOne executing...\n" ) ) ;
    EnterCriticalSection ( &g_csA ) ;
    _tprintf ( _T ( "CriticalSectionThreadOne acquired CS A...\n" ) ) ;
    Sleep ( 500 ) ;
    EnterCriticalSection ( &g_csB ) ;
    _tprintf ( _T ( "CriticalSectionThreadOne acquired CS B...\n" ) ) ;
    Sleep ( 500 ) ;
    LeaveCriticalSection ( &g_csB ) ;
    LeaveCriticalSection ( &g_csA ) ;
    _tprintf ( _T ( "CriticalSectionThreadOne exiting...\n" ) ) ;
    return  ( 1 ) ;
}

DWORD __stdcall CriticalSectionThreadTwo ( DWORD )
{
    _tprintf ( _T ( "CriticalSectionThreadTwo executing...\n" ) ) ;
    EnterCriticalSection ( &g_csB ) ;
    _tprintf ( _T ( "CriticalSectionThreadTwo acquired CS B...\n" ) ) ;
    Sleep ( 500 ) ;
    EnterCriticalSection ( &g_csA ) ;
    _tprintf ( _T ( "CriticalSectionThreadTwo acquired CS A...\n" ) ) ;
    Sleep ( 500 ) ;
    LeaveCriticalSection ( &g_csA ) ;
    LeaveCriticalSection ( &g_csB ) ;
    _tprintf ( _T ( "CriticalSectionThreadTwo exiting...\n" ) ) ;
    return  ( 1 ) ;
}

void CriticalSectionDeadlock ( ) 
{
    _tprintf ( _T ( "Deadlocking on CRITICAL_SECTIONs\n" ) ) ;
    InitializeCriticalSectionAndSpinCount ( &g_csA , 4000 ) ;
    InitializeCriticalSectionAndSpinCount ( &g_csB , 4000 ) ;
    ThreadSpinner ( (PSTARTADDR)CriticalSectionThreadOne , 
                    (PSTARTADDR)CriticalSectionThreadTwo  ) ;
}

////////////////////////////////////////////////////////////////////////////////
// CriticalSection Deadlock End
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Mutex Deadlock Begin
////////////////////////////////////////////////////////////////////////////////

static HANDLE g_hMutexA ;

DWORD __stdcall MutexThreadOne ( DWORD )
{
    _tprintf ( _T ( "MutexThreadOne executing...\n" ) ) ;
    WaitForSingleObject ( g_hMutexA , INFINITE ) ;
    _tprintf ( _T ( "MutexThreadOne acquired Mutex A...\n" ) ) ;
    ReleaseMutex ( g_hMutexA ) ;
    _tprintf ( _T ( "MutexThreadOne exiting...\n" ) ) ;
    return  ( 1 ) ;
}

DWORD __stdcall MutexThreadTwo ( DWORD )
{
    _tprintf ( _T ( "MutexThreadTwo executing...\n" ) ) ;
    WaitForSingleObject ( g_hMutexA , INFINITE ) ;
    _tprintf ( _T ( "MutexThreadTwo acquired Mutex A...\n" ) ) ;
    ReleaseMutex ( g_hMutexA ) ;
    _tprintf ( _T ( "MutexThreadTwo exiting...\n" ) ) ;
    return  ( 1 ) ;
}

void MutexDeadlock ( ) 
{
    g_hMutexA = CreateMutex ( NULL , TRUE , _T ( "Deadlock Mutex A" ) ) ;
    ThreadSpinner ( (PSTARTADDR)MutexThreadOne , 
                    (PSTARTADDR)MutexThreadTwo  ) ;
    ReleaseMutex ( g_hMutexA ) ;
}

////////////////////////////////////////////////////////////////////////////////
// Mutex Deadlock End
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Mutex WaitForSingleThread Start
////////////////////////////////////////////////////////////////////////////////

void MutexDeadlockWaitForSingleObject ( ) 
{
    g_hMutexA = CreateMutex ( NULL , TRUE , _T ( "Deadlock Mutex A" ) ) ;

    DWORD dwThreadId ;
    HANDLE hThread1  ;
    HANDLE hThread2  ;

    hThread1 = (HANDLE) _beginthreadex ( NULL                       ,
                                         0                          ,
                                         (PSTARTADDR)MutexThreadOne ,
                                         (LPVOID)0                  ,
                                         0                          ,
                                         (unsigned*)&dwThreadId      ) ;

    hThread2 = (HANDLE) _beginthreadex ( NULL                       ,
                                         0                          ,
                                         (PSTARTADDR)MutexThreadTwo ,
                                         (LPVOID)0                  ,
                                         0                          ,
                                         (unsigned*)&dwThreadId      ) ;
    _tprintf (  _T ( "Main thread waiting for background threads to end...\n"));
    WaitForSingleObject ( hThread1 , INFINITE ) ;
    WaitForSingleObject ( hThread2 , INFINITE ) ;
    ReleaseMutex ( g_hMutexA ) ;
}


////////////////////////////////////////////////////////////////////////////////
// Mutex WaitForSingleThread End
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// SendMessage and Critical Section Begin
////////////////////////////////////////////////////////////////////////////////

static CRITICAL_SECTION g_csC ;
static HWND g_hWnd = NULL ;

#define WM_DEADLOCK WM_USER + 1 

LRESULT CALLBACK MainWndProc ( HWND   hWnd   , 
                               UINT   msg    , 
                               WPARAM wParam , 
                               LPARAM lParam  )
{
    switch ( msg )
    {
        case WM_DESTROY :
            PostQuitMessage ( 0 ) ;
            return ( 0 ) ;
            break ;
        case WM_DEADLOCK :
            EnterCriticalSection ( &g_csC ) ;
            _tprintf ( _T ( "Now processing WM_DEADLOCK message.\n" ) ) ;
            LeaveCriticalSection ( &g_csC ) ;
            return ( 0 ) ;
            break ;
    }
    return ( DefWindowProc ( hWnd  , msg , wParam , lParam ) ) ;
}

DWORD __stdcall WindowOnThread ( DWORD )
{
    g_hWnd = CreateWindow ( _T ( "MainWClass" )      ,
                            _T ( "Deadlocker" )      ,
                            WS_OVERLAPPEDWINDOW      ,
                            CW_USEDEFAULT            ,
                            CW_USEDEFAULT            ,
                            CW_USEDEFAULT            ,
                            CW_USEDEFAULT            ,
                            (HWND) NULL              ,
                            (HMENU) NULL             ,
                            GetModuleHandle ( NULL ) ,
                            (LPVOID) NULL             ) ;
    if ( NULL != g_hWnd )
    {
        _tprintf ( _T ( "Window created and starting to process messages\n" ) );

        ShowWindow ( g_hWnd , SW_HIDE ) ;
        UpdateWindow( g_hWnd ) ; 

        MSG msg ;
        while ( TRUE == GetMessage ( &msg , (HWND)NULL , 0 , 0 ) )
        { 
            TranslateMessage ( &msg ) ; 
            DispatchMessage ( &msg ) ; 
        } 
    }
    else
    {
        _tprintf ( _T ( "Unable to create the window!\n" ) ) ;
    }
    return ( 0 ) ;
}

DWORD __stdcall SenderThread ( DWORD )
{
    EnterCriticalSection ( &g_csC ) ;
    _tprintf ( _T ( "Critical section grabbed and now sending message\n" ) ) ;
    SendMessage ( g_hWnd , WM_DEADLOCK , 0 , 0 ) ;
    LeaveCriticalSection ( &g_csC ) ;
    return ( 0 ) ;
}

void SendMessageAndCriticalSectionDeadlock ( )
{

    _tprintf ( _T ( "Initializing critical section\n" ) ); 
    InitializeCriticalSectionAndSpinCount ( &g_csC , 4000 ) ;

    WNDCLASS wc ; 
    wc.style         = CS_HREDRAW | CS_VREDRAW ;
    wc.lpfnWndProc   = MainWndProc ;
    wc.cbClsExtra    = 0 ;
    wc.cbWndExtra    = 0 ;
    wc.hInstance     = GetModuleHandle ( NULL ) ;         
    wc.hIcon         = LoadIcon ( NULL , IDI_APPLICATION ) ;
    wc.hCursor       = LoadCursor ( NULL , IDC_ARROW ) ;
    wc.hbrBackground = (HBRUSH)GetStockObject ( WHITE_BRUSH ) ;
    wc.lpszMenuName  = NULL ;
    wc.lpszClassName = _T ( "MainWClass" ) ;
    if ( 0 != RegisterClass ( &wc ) )
    {
        DWORD dwThreadId ;
        // Create the window on another thread.
        HANDLE hThread = (HANDLE) _beginthreadex ( NULL                      ,
                                                   0                         ,
                                                  (PSTARTADDR)WindowOnThread ,
                                                  (LPVOID)0                  ,
                                                  0                          ,
                                                  (unsigned*)&dwThreadId      );
        // Let the window get created.
        while ( NULL == g_hWnd ) 
        {
        }

        HANDLE hThread2 = (HANDLE) _beginthreadex ( NULL                   ,
                                                   0                       ,
                                                  (PSTARTADDR)SenderThread ,
                                                  (LPVOID)0                ,
                                                  0                        ,
                                                  (unsigned*)&dwThreadId    ) ;

        WaitForSingleObject ( hThread2 , INFINITE ) ; 
        WaitForSingleObject ( hThread , INFINITE ) ;
    }
    else
    {
        _tprintf ( _T ( "Failed to register window." ) ) ;
    }
}

////////////////////////////////////////////////////////////////////////////////
// SendMessage and Critical Section End
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Process Deadlock Start
////////////////////////////////////////////////////////////////////////////////

void ProcessDeadlock ( )
{
    HANDLE hToolProcs = CreateToolhelp32Snapshot ( TH32CS_SNAPPROCESS , 0 ) ;
    if ( INVALID_HANDLE_VALUE != hToolProcs )
    {
        PROCESSENTRY32  pe32 ;
        pe32.dwSize = sizeof ( PROCESSENTRY32 ) ;
        BOOL bRet = Process32First ( hToolProcs , &pe32 ) ;
        while ( TRUE == bRet )
        {
            if ( 0 == _tcsicmp ( _T ( "explorer.exe" ) , pe32.szExeFile ) )
            {
                HANDLE hExplorer = OpenProcess ( PROCESS_ALL_ACCESS , 
                                                 FALSE , 
                                                 pe32.th32ProcessID ) ;
                if ( NULL != hExplorer )
                {
                    _tprintf ( _T ( "About to wait on Explorer.exe\n" ) ) ;
                    WaitForSingleObject ( hExplorer , INFINITE ) ;
                    CloseHandle ( hExplorer ) ;
                }
                else
                {
                }
            }
            bRet = Process32Next ( hToolProcs , &pe32 ) ;
        }

        CloseHandle ( hToolProcs ) ;
    }
    else
    {
        _tprintf ( _T ( "Unable to get toolhelp process snap shot.\n" ) ) ;
    }
}

////////////////////////////////////////////////////////////////////////////////
// Process Deadlock End
////////////////////////////////////////////////////////////////////////////////

// Show what this program does. :)
void ShowUsage ( void )
{
    _tprintf ( _T ( "Dead - A program to create deadlocks.\n" ) ) ;
    _tprintf ( _T ( "Usage:\n" ) ) ;
    _tprintf ( _T ( "    -c - Deadlock on critical sections\n" ) ) ;
    _tprintf ( _T ( "    -m - Deadlock on mutexes\n" ) ) ;
    _tprintf ( _T ( "    -n - Deadlock on mutexes (WaitForSingleEvent)\n" ) ) ;
    _tprintf ( _T ( "    -s - Deadlock on a CS and SendMessage\n" ) ) ;
    _tprintf ( _T ( "    -p - Deadlock on the Explorer process\n" ) ) ;
}

// Resolve all the command line stuff.
BOOL ResolveCommandLine ( OPTIONS & stOpts , int argc , TCHAR * argv[] )
{
    // Set the options to the defaults.
    memset ( &stOpts , NULL , sizeof ( OPTIONS ) ) ;

    // Check the easy case.
    if ( 1 == argc )
    {
        stOpts.bShowHelp = TRUE ;
        return ( FALSE ) ;
    }

    for ( int i = 1 ; i < argc ; i++ )
    {
        LPTSTR szString = argv[ i ] ;

        switch ( *szString )
        {
            case _T ( '/' ) :
            case _T ( '-' ) :
            {
                switch (  _totupper ( *(++szString) ) )
                {
                    case _T ( 'C' ) :
                        stOpts.bCriticalSection = TRUE ;
                    break ;
                    case _T ( 'M' ) :
                        stOpts.bMutex = TRUE ;
                    break ;
                    case _T ( 'N' ) :
                        stOpts.bMutexSingleThread = TRUE ;
                    break;
                    case _T ( 'S' ) :
                        stOpts.bCriticalSectionSendMessage = TRUE ;
                    break;
                    case _T ( 'P' ) :
                        stOpts.bProcess = TRUE ;
                    break ;
                    case _T ( '?' ) :
                        stOpts.bShowHelp = TRUE ;
                        return ( TRUE ) ;
                    break ;
                    default         :
                        stOpts.bShowHelp = TRUE ;
                        return ( FALSE ) ;
                    break ;
                }
            }
            break ;
        }
    }
    return ( TRUE ) ;
}
