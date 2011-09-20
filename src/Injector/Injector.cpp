#include <string>
#include <iostream>
#include <windows.h>
#include <conio.h>

using namespace std;

void AddDebugPrivilege();
void InjectDll( DWORD dwProcessId, LPCSTR lpszDLLPath );

void main( int argc, char* argv[] )
{
    cout << "Adding debug privilege to current process..." << endl;
    AddDebugPrivilege();

    int process = atoi( argv[1] );
    LPCSTR injectedDll = argv[2];
    cout << "Injecting dll: " << injectedDll << endl;
    cout << "To process ID: " << process << endl;
    InjectDll( process, injectedDll );

    cout << "Dll was successfully injected." << endl;
    if( 1 == argc )
    {
        cout << 
            "SYNOPSIS" << endl <<
            "   Inject DLL into a process. Remote thread technique is used." << endl <<
            "" << endl <<
            "SYNTAX" << endl <<
            "   Injector.exe <PID> <DLL path>" << endl <<
            "   <PID>       - Process ID. Process that is to be injected with DLL." << endl <<
            "   <DLL Path>  - Path to injected DLL." << endl <<
            "" << endl <<
            "EXAMPLE" << endl << 
            "   Injector 123 MyHook.dll" << endl;
        return 1;
    }

    try
    {
        AddDebugPrivilege();
        cout << "Added debug privilege to current process." << endl;

        int process = atoi( argv[1] );
        LPCSTR injectedDll = argv[2];
        cout << "Injected: " << injectedDll << endl;
        cout << "Process: " << process << endl;
        InjectDll( process, injectedDll );

        cout << "Dll was successfully injected." << endl;
    }
    catch( ... )
    {
        cout << "Unknown error. Run without parameters to see help." << endl;
        return 1;
    }
    
    _getch();
}

void AddDebugPrivilege()
{ 
    HANDLE token;
    HANDLE injector = GetCurrentProcess();
    TOKEN_PRIVILEGES privileges;

    if( 0 == OpenProcessToken( injector, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &token ) )
    {
        cout << "OpenProcessToken failed with code: " << GetLastError() << endl;
        exit( 1 );
    }

    if( 0 == LookupPrivilegeValue( NULL, SE_DEBUG_NAME, &privileges.Privileges[0].Luid ) )
    {
        cout << "LookupPrivilegeValue failed with code: " << GetLastError() << endl;
        exit( 1 );
    }

    privileges.PrivilegeCount = 1;
    privileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 

    if( 0 == AdjustTokenPrivileges( token, 0, &privileges, sizeof(privileges), NULL, NULL ) )
    {
        cout << "AdjustTokenPrivileges failed with code: " << GetLastError() << endl;
        exit( 1 );
    }
}   

void InjectDll( DWORD processId, LPCSTR dllPath )
{
    HANDLE hProcess = OpenProcess(
        PROCESS_CREATE_THREAD|
        PROCESS_QUERY_INFORMATION|
        PROCESS_VM_OPERATION|
        PROCESS_VM_WRITE|
        PROCESS_VM_READ, 
        FALSE, 
        processId);
    if( NULL == hProcess )
    {
        cout << "OpenProcess failed with code: " << GetLastError() << endl;
        exit(1);
    }

    DWORD dwMemSize = lstrlenA(dllPath) + 1;
    LPVOID lpBaseAddr = VirtualAllocEx( hProcess, NULL, dwMemSize, MEM_COMMIT, PAGE_READWRITE );
    if( NULL == lpBaseAddr )
    {
        cout << "VirtualAllocEx failed with code: " << GetLastError() << endl;
        exit(1);
    }

    BOOL memoryWriteSuccess = WriteProcessMemory( hProcess, lpBaseAddr, dllPath, dwMemSize, NULL );
    if( 0 == memoryWriteSuccess )
    {
        cout << "WriteProcessMemory failed with code: " << GetLastError() << endl;
        exit(1);
    }

    HMODULE hUserDLL = LoadLibrary( TEXT("kernel32.dll") );
    if( NULL == hUserDLL )
    {
        cout << "LoadLibrary failed with code: " << GetLastError() << endl;
        exit(1);
    }

    LPVOID lpFuncAddr = GetProcAddress( hUserDLL, "LoadLibraryA" );
    if( NULL == lpFuncAddr )
    {
        cout << "GetProcAddress failed with code: " << GetLastError() << endl;
        exit(1);
    }

    HANDLE hThread = CreateRemoteThread( hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)lpFuncAddr, lpBaseAddr, 0, NULL );
    if( NULL == hThread )
    {
        cout << "CreateRemoteThread failed with code: " << GetLastError() << endl;
        exit(1);
    }

    DWORD waitResult = WaitForSingleObject( hThread, INFINITE );
    if( WAIT_OBJECT_0 != waitResult )
    {
        cout << "WaitForSingleObject failed. Status: " << waitResult << ". Last error: " << GetLastError() << endl;
        exit(1);
    }

    DWORD dwExitCode;
    BOOL exitThreadResult = GetExitCodeThread( hThread, &dwExitCode );
    if( 0 == exitThreadResult )
    {
        cout << "GetExitCodeThread failed with code: " << GetLastError() << endl;
        exit(1);
    }

    if( 0 == dwExitCode )
    {
        cout << "Remote LoadLibrary failed with code: " << GetLastError() << endl;
        exit(1);
    }

    CloseHandle( hThread );
    FreeLibrary( hUserDLL );
    VirtualFreeEx( hProcess, lpBaseAddr, 0, MEM_RELEASE );
    CloseHandle( hProcess );
}
