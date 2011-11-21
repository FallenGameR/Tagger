#include "stdafx.h"
#include "process.h"

TEST( Cmd_is_console_app_Test )
{
    TCHAR* cmdPath = TEXT("C:\\Windows\\system32\\cmd.exe");
    IsConsoleApp(cmdPath);
}

TEST( Powershell_is_console_app_Test )
{
    TCHAR* cmdPath = TEXT("C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe");
    IsConsoleApp(cmdPath);
}

TEST( Notepad_is_not_console_app_Test )
{
    TCHAR* cmdPath = TEXT("C:\\Windows\\system32\\notepad.exe");
    IsConsoleApp(cmdPath);
}
