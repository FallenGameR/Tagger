#include "stdafx.h"
#include "process.h"

TEST( Wct_call_Test )
{
    int returnCode = UsingWctMain( 0, NULL );
    CHECK( 0 != returnCode );
}