#include "stdafx.h"
#include "process.h"

TEST( Cmd_is_console_app_Test )
{
    TCHAR* path = TEXT("C:\\Windows\\system32\\cmd.exe");
    bool isCui = IsConsoleApp(path);
    CHECK( isCui );    
}

TEST( Powershell_is_console_app_Test )
{
    TCHAR* path = TEXT("C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe");
    bool isCui = IsConsoleApp(path);
    CHECK( isCui );    
}

TEST( Notepad_is_not_console_app_Test )
{
    TCHAR* path = TEXT("C:\\Windows\\system32\\notepad.exe");
    bool isCui = IsConsoleApp(path);
    CHECK( !isCui );    
}
