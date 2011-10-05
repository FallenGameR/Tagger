#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>
#include <tchar.h>
#include <memory>
#include "Hook.h"

using namespace std;



class Process
{
public:
    Process( LPTSTR path )
    {

    }

    ~Process()
    {

    }

private:

};

HANDLE g_process = NULL;
HANDLE g_thread = NULL;


void MyCreateProcess( LPTSTR path )
{
    size_t length = _tcslen( path ) + 1;
    auto_ptr<TCHAR> path_copy( new TCHAR[ length ] );
    _tcscpy_s( path_copy.get(), length, path );

    STARTUPINFO si;
    PROCESS_INFORMATION pi;

    ZeroMemory( &si, sizeof(si) );
    ZeroMemory( &pi, sizeof(pi) );

    si.cb = sizeof(si);

    BOOL success = CreateProcess( NULL, path_copy.get(), NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi );
    if( !success ) { throw error("CreateProcess"); }

    g_process = pi.hProcess;
    g_thread = pi.hThread;
}


int main( int argc, char* argv[] )
{
    try
    {
        // TODO: Verify that console window can be hooked as well.
        MyCreateProcess( TEXT("Host.Windows.exe") );

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

        BOOL success = TerminateProcess( g_process, 0 );
        if( !success ) { throw error("TerminateProcess"); }

        success = CloseHandle( g_process );
        if( !success ) { throw error("CloseHandle - process"); }

        success = CloseHandle( g_thread );
        if( !success ) { throw error("CloseHandle - thread"); }
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

