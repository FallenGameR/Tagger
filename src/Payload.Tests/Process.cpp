#include "stdafx.h"
#include "Process.h"
#include "WinApiException.h"

Process::Process( LPTSTR processPath )
{
    // Initializing structures
    ZeroMemory( &m_StartupInfo, sizeof(m_StartupInfo) );
    ZeroMemory( &m_ProcessInformation, sizeof(m_ProcessInformation) );
    m_StartupInfo.cb = sizeof(m_StartupInfo);

    // CreateProcessW modifies path string thus we need to copy path string
    size_t length = _tcslen( processPath ) + 1;
    auto_ptr<TCHAR> path_copy( new TCHAR[ length ] );
    _tcscpy_s( path_copy.get(), length, processPath );

    // Create the process
    BOOL success = CreateProcess( path_copy.get(), NULL, NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL, NULL, &m_StartupInfo, &m_ProcessInformation );
    if( !success ) { throw WinApiException("CreateProcess"); }
}

Process::~Process()
{
    TerminateProcess( m_ProcessInformation.hProcess, 0 );
    CloseHandle( m_ProcessInformation.hProcess );
    CloseHandle( m_ProcessInformation.hThread );
}

HWND Process::FindMainWindow( LPTSTR className, LPTSTR title )
{
    // Probe constants are taken in milliseconds
    int probeLength = 10 * 1000;    // 10 seconds of trying
    int probeInterval = 100;        // probe each 100 ms

    for( int i = 0; i < probeLength; i += probeInterval )
    {
        HWND handle = FindWindow( className, title );
        if( NULL != handle ) { return handle; }
        Sleep( probeInterval );
    }

    throw WinApiException("FindWindow");
}

DWORD Process::GetPid()
{
    return m_ProcessInformation.dwProcessId;
}