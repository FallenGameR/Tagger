#include "stdafx.h"
#include "process.h"

TEST( Wct_call_Test )
{
    //DWORD pid = GetCurrentProcessId();
    int returnCode = UsingWctMain( 1880 );
    CHECK( 0 != returnCode );
}