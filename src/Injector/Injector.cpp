#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>
#include "Hook.h"

using namespace std;

int main( int argc, char* argv[] )
{
    try
    {
        // TODO: Verify that console window can be hooked as well.

        // Parse parameters
        LPCSTR injectedDll = "Hook.dll";

        // Try hook call
        HWND handle = FindWindow( TEXT("TARGET"), TEXT("TARGET") );
        if( NULL == handle ) { throw error("GetForegroundWindow"); }

        char buffer[128];
        InjectDll_HookMessageQueue( handle, buffer );
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

