// Standard headers
#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>
#include <tchar.h>
#include <memory>

using namespace std;

// Dependencies
#include "Hook.h"

// Current project
#include "Process.hpp"

int main( int argc, char* argv[] )
{
    // TODO: Use unit test library to test for injection
    // TODO: Remove ps1 tests if not needed
    // TODO: Add precompiled header to tests
    // TODO: Write test that covers console scenario

    try
    {
        Process process( TEXT("Host.Windows.exe") );

        // Parse parameters
        LPCSTR injectedDll = "Hook.dll";

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
        cout << "Hook successfully deployed." << endl;

    }
    catch( error &er )
    {
        cout << "ERROR. " << er.Message << ": " << er.ErrorCode << endl;
        system("pause");
        return 1;
    }
    catch( ... )
    {
        cout << "Unknown error. Run without parameters to see help." << endl;
        system("pause");
        return 1;
    }
    
    system("pause");
    return 0;
}

