#include "stdafx.h"
#include "process.h"

TEST( Hook_console_window )
{
    Process process( TEXT("Host.Console.exe") );
    HWND handle = process.FindMainWindow( TEXT("ConsoleWindowClass"), TEXT("Console Target x86") );

    //throw ProgramException("test", 9);

    // http://stackoverflow.com/questions/4706590/changing-window-procedure-for-console-window
    // http://blogs.msdn.com/b/oldnewthing/archive/2007/12/31/6909007.aspx
    // http://blogs.technet.com/b/askperf/archive/2009/10/05/windows-7-windows-server-2008-r2-console-host.aspx
    // 

    char buffer[128];
    InjectDll_HookMessageQueue( handle, buffer );
}