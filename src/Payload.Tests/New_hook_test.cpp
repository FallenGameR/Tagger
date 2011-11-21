#include "stdafx.h"
#include "process.h"

TEST( New_hook_test )
{
    //Process process( TEXT("Host.Windows.exe") );   
    //StartHooking( process.GetPid() );

    Process process( TEXT("Host.Console.exe") );
    DWORD pid = FindConhost( process.GetPid() );
    StartHooking( pid );
}