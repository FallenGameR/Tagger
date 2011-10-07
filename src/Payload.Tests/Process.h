#pragma once
#include "stdafx.h"

class Process
{
public:
    Process( LPTSTR processPath );
    ~Process();
    HWND FindMainWindow( LPTSTR className, LPTSTR title );

private:
    STARTUPINFO m_StartupInfo;
    PROCESS_INFORMATION m_ProcessInformation;
};
