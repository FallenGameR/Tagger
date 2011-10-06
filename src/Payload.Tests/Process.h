#pragma once
#include "stdafx.h"

class Process
{
public:
    Process( LPTSTR processPath );
    ~Process();

private:
    STARTUPINFO m_StartupInfo;
    PROCESS_INFORMATION m_ProcessInformation;
};
