#pragma once

class Process
{
public:
    Process( LPTSTR processPath )
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
        BOOL success = CreateProcess( NULL, path_copy.get(), NULL, NULL, FALSE, 0, NULL, NULL, &m_StartupInfo, &m_ProcessInformation );
        if( !success ) { throw error("CreateProcess"); }
    }

    ~Process()
    {
        TerminateProcess( m_ProcessInformation.hProcess, 0 );
        CloseHandle( m_ProcessInformation.hProcess );
        CloseHandle( m_ProcessInformation.hThread );
    }

private:
    STARTUPINFO m_StartupInfo;
    PROCESS_INFORMATION m_ProcessInformation;
};