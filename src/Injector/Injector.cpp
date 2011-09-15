#include <string>
#include <iostream>
#include <windows.h>

#define MAXWAIT 10000

bool insertDll(DWORD procID, std::string dll);

void main( int argc, char* argv[] )
{
    insertDll( 4624, "d:\\Code\\Tagger\\bin\\Debug\\Hook.dll" );
}

bool insertDll(DWORD procID, std::string dll)
{
    // Find LoadLibrary address (it is loaded on the same address for every process)
    HMODULE hKernel32 = GetModuleHandle( L"Kernel32" );
    FARPROC hLoadLibrary = GetProcAddress( hKernel32, "LoadLibraryW" );
    
    // Add debug privilege to current process
    HANDLE hToken;
    TOKEN_PRIVILEGES tkp;
    if( OpenProcessToken( GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken ) )
    {
        LookupPrivilegeValue( NULL, SE_DEBUG_NAME, &tkp.Privileges[0].Luid );
        tkp.PrivilegeCount = 1;
        tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
        bool success = AdjustTokenPrivileges( hToken, 0, &tkp, sizeof(tkp), NULL, NULL );
        std::cout << "Token adjusted: " << success << std::endl;
    }

    // Open the process with all access
    HANDLE hProc = OpenProcess( PROCESS_ALL_ACCESS, FALSE, procID );

    //Allocate memory to hold the path to the Dll File in the process's memory
    dll += '\0';
    LPVOID hRemoteMem = VirtualAllocEx(hProc, NULL, dll.size(), MEM_COMMIT, PAGE_READWRITE);

    //Write the path to the Dll File in the location just created
    DWORD numBytesWritten;
    WriteProcessMemory(hProc, hRemoteMem, dll.c_str(), dll.size(), &numBytesWritten);

    //Create a remote thread that starts begins at the LoadLibrary function and is passed are memory pointer
    HANDLE hRemoteThread = CreateRemoteThread(hProc, NULL, 0, (LPTHREAD_START_ROUTINE)hLoadLibrary, hRemoteMem, 0, NULL);

    std::cout << hRemoteThread << std::endl;

    //Wait for the thread to finish
    bool res = false;
    if (hRemoteThread)
        res = (bool)WaitForSingleObject(hRemoteThread, MAXWAIT) != WAIT_TIMEOUT;

    //Free the memory created on the other process
    VirtualFreeEx(hProc, hRemoteMem, dll.size(), MEM_RELEASE);

    //Release the handle to the other process
    CloseHandle(hProc);

    return res;
}
