#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>
#include <sys/stat.h>
#include "Hook.h"

using namespace std;

int main( int argc, char* argv[] )
{
    if( 1 == argc )
    {
        cout << 
            "SYNOPSIS" << endl <<
            "   Inject DLL into a process. Remote thread technique is used." << endl <<
            "" << endl <<
            "SYNTAX" << endl <<
            "   Injector.exe <PID> <DLL path>" << endl <<
            "   <PID>       - Process ID. Process that is to be injected with DLL. Use 0 to attach Injector to itself." << endl <<
            "   <DLL Path>  - Path to injected DLL." << endl <<
            "" << endl <<
            "EXAMPLE" << endl << 
            "   Injector 123 MyHook.dll" << endl;
        return 1;
    }

    try
    {
        // Add debug privilege
        // TODO: Verify that adjusting privileges is needed. Check running without studio and hooking admin-run process.
        // TODO: Verify that console window can be hooked as well.
        AddDebugPrivilege();
        cout << "Added debug privilege to current process." << endl;

        // Parse parameters
        int process = atoi( argv[1] );
        LPCSTR injectedDll = argv[2];

        // Validate parameters
        if( 0 == process )
        {
            process = GetCurrentProcessId();
        }

        struct stat file_stat;
        if( 0 != stat( injectedDll, &file_stat ) )
        {
            throw error("Injected DLL file doesn't exist");
        }

        // Inject DLL       
        cout << "Injected: " << injectedDll << endl;
        cout << "Process: " << process << endl;
        InjectDll_CreateRemoteThread( process, injectedDll );
        cout << "DLL was successfully injected." << endl;

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

