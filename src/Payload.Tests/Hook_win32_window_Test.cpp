#include "stdafx.h"
#include "Process.h"

TEST( Hook_win32_window )
{
    Process process( TEXT("Host.Windows.exe") );
    HWND handle = process.FindMainWindow( TEXT("TARGET"), TEXT("TARGET") );

    char buffer[128];
    InjectDll_HookMessageQueue( handle, buffer );
}