// Standard headers
#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>
#include <tchar.h>
#include <memory>

#include <UnitTest++.h>

using namespace std;

// Dependencies
#include "Hook.h"

// Current project
#include "Process.hpp"

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

int main( int argc, char* argv[] )
{
    // TODO: Use unit test library to test for injection
    // TODO: Remove ps1 tests if not needed
    // TODO: Add precompiled header to tests
    // TODO: Write test that covers console scenario
    // TODO: Use separate library for exceptions
    // TODO: /W warning level for all projects.
    // TODO: /Analyse works without any warnings. On Library code. Tests are fine unanalysed.
    // TODO: Use incremental build. See default project settings.

    return UnitTest::RunAllTests();
}

