#include "stdafx.h"
#include "process.h"

TEST( New_hook_test )
{
    Process process( TEXT("Host.Windows.exe") );   
    StartHooking( process.GetPid() );
}