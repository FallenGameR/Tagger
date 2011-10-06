#include "stdafx.h"
#include "Process.h"

TEST( Hook_win32_window )
{
    Process process( TEXT("Host.Windows.exe") );

    // Try hook call
    HWND handle = NULL;
    while( NULL == handle )
    {
        handle = FindWindow( TEXT("TARGET"), TEXT("TARGET") );
        Sleep(10);
    }        
    if( NULL == handle ) { throw error("FindWindow"); }

    char buffer[128];
    InjectDll_HookMessageQueue( handle, buffer );
}