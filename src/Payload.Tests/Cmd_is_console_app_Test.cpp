#include "stdafx.h"
#include "process.h"

TEST( Cmd_is_console_app_Test )
{
    Process process( TEXT("Host.Console.exe") );
    HWND handle = process.FindMainWindow( TEXT("ConsoleWindowClass"), TEXT("Console Target x86") );



    char buffer[128];
    InjectDll_HookMessageQueue( handle, buffer );
}