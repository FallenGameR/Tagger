#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>
#include <sys/stat.h>
#include "Hook.h"

using namespace std;

void AddDebugPrivilege();
void InjectDll_CreateRemoteThread( DWORD dwProcessId, LPCSTR lpszDLLPath );

struct error
{
    char* Message;
    DWORD ErrorCode;

    error( char* message ): Message(message), ErrorCode(GetLastError()) {}
    error( char* message, DWORD lastError): Message(message), ErrorCode(lastError) {}
};

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

void AddDebugPrivilege()
{ 
    HANDLE token;
    HANDLE injector = GetCurrentProcess();
    TOKEN_PRIVILEGES privileges;

    if( 0 == OpenProcessToken( injector, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &token ) ) { throw error("OpenProcessToken"); }
    if( 0 == LookupPrivilegeValue( NULL, SE_DEBUG_NAME, &privileges.Privileges[0].Luid ) ) { throw error("LookupPrivilegeValue"); }

    privileges.PrivilegeCount = 1;
    privileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 

    if( 0 == AdjustTokenPrivileges( token, 0, &privileges, sizeof(privileges), NULL, NULL ) ) { throw error("AdjustTokenPrivileges"); }
}   

void InjectDll_CreateRemoteThread( DWORD processId, LPCSTR dllPath )
{
    // TODO: This hook doesn't unload injected DLL itself. But it should.
    // TODO: Check what OpenProcess flags are really required.

    HANDLE hProcess = NULL;
    LPVOID lpBaseAddr = NULL;
    HMODULE hKernel32Dll = NULL;
    HANDLE hThread = NULL;

    try
    {
        hProcess = OpenProcess( PROCESS_CREATE_THREAD|PROCESS_QUERY_INFORMATION|PROCESS_VM_OPERATION|PROCESS_VM_WRITE|PROCESS_VM_READ, FALSE, processId );
        if( NULL == hProcess ) { throw error("OpenProcess"); }

        DWORD dwMemSize = lstrlenA(dllPath) + 1;
        lpBaseAddr = VirtualAllocEx( hProcess, NULL, dwMemSize, MEM_COMMIT, PAGE_READWRITE );
        if( NULL == lpBaseAddr ) { throw error("VirtualAllocEx"); }

        BOOL memoryWriteSuccess = WriteProcessMemory( hProcess, lpBaseAddr, dllPath, dwMemSize, NULL );
        if( 0 == memoryWriteSuccess ) { throw error("WriteProcessMemory"); }

        hKernel32Dll = LoadLibrary( TEXT("kernel32.dll") );
        if( NULL == hKernel32Dll ) { throw error("LoadLibrary"); }

        LPVOID lpFuncAddr = GetProcAddress( hKernel32Dll, "LoadLibraryA" );
        if( NULL == lpFuncAddr ) { throw error("GetProcAddress"); }

        hThread = CreateRemoteThread( hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)lpFuncAddr, lpBaseAddr, 0, NULL );
        if( NULL == hThread ) { throw error("CreateRemoteThread"); }

        DWORD waitResult = WaitForSingleObject( hThread, INFINITE );
        if( WAIT_FAILED == waitResult ) { throw error("WaitForSingleObject"); }
        if( WAIT_OBJECT_0 != waitResult ) { throw error("WaitForSingleObject returned", waitResult ); }

        DWORD dwExitCode;
        BOOL exitThreadResult = GetExitCodeThread( hThread, &dwExitCode );
        if( 0 == exitThreadResult ) { throw error("GetExitCodeThread"); }
        if( 0 == dwExitCode ) { throw error("Remote LoadLibrary returned", GetLastError()); }
    }
    catch( error & )
    {
        if( NULL != hThread )  { CloseHandle( hThread ); }
        if( NULL != hKernel32Dll ) { FreeLibrary( hKernel32Dll ); }
        if( NULL != lpBaseAddr ) { VirtualFreeEx( hProcess, lpBaseAddr, 0, MEM_RELEASE ); }
        if( NULL != hProcess ) { CloseHandle( hProcess ); }

        throw;
    }

    if( NULL != hThread )  { CloseHandle( hThread ); }
    if( NULL != hKernel32Dll ) { FreeLibrary( hKernel32Dll ); }
    if( NULL != lpBaseAddr ) { VirtualFreeEx( hProcess, lpBaseAddr, 0, MEM_RELEASE ); }
    if( NULL != hProcess ) { CloseHandle( hProcess ); }
}
