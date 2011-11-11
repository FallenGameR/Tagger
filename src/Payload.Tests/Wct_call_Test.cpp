#include "stdafx.h"
#include "process.h"

TEST( Wct_call_Test )
{
    Process process( TEXT("Host.Console.exe") );
    int returnCode = FindConhost( process.GetPid() );
    CHECK( 0 != returnCode );
}