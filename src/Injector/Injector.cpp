#include <string>
#include <iostream>
#include <windows.h>

#define MAXWAIT 10000

bool insertDll(DWORD procID, std::string dll);
BOOL InjectDLL(DWORD dwProcessId, LPCSTR lpszDLLPath);
bool InjectDll(char *dllName, DWORD ProcessID);

void main( int argc, char* argv[] )
{
    InjectDLL( 6968, "c:\\src\\github\\Tagger\\playground\\HookSpy\\Debug\\HookSpyDll.dll" );
    //InjectDll( "c:\\Program Files (x86)\\totalcmd\\CABRK.DLL", 6968 );
}

bool GimmePrivileges(){ 
    HANDLE Token;  
    TOKEN_PRIVILEGES tp;      
    if(OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &Token)    )
    { 
        LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &tp.Privileges[0].Luid);     
        tp.PrivilegeCount = 1;
        tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 
        AdjustTokenPrivileges(Token, 0, &tp, sizeof(tp), NULL, NULL);    
        return true;
    }      
    return false;
}   

BOOL InjectDLL(DWORD dwProcessId, LPCSTR lpszDLLPath)
{

    HANDLE  hProcess, hThread;
    LPVOID  lpBaseAddr, lpFuncAddr;
    DWORD   dwMemSize, dwExitCode;
    BOOL    bSuccess = FALSE;
    HMODULE hUserDLL;

    GimmePrivileges();

    if((hProcess = OpenProcess(PROCESS_CREATE_THREAD|PROCESS_QUERY_INFORMATION|PROCESS_VM_OPERATION
        |PROCESS_VM_WRITE|PROCESS_VM_READ, FALSE, dwProcessId)))
    {
        dwMemSize = lstrlenA(lpszDLLPath) + 1;
        if(lpBaseAddr = VirtualAllocEx(hProcess, NULL, dwMemSize, MEM_COMMIT, PAGE_READWRITE))
        {
            if(WriteProcessMemory(hProcess, lpBaseAddr, lpszDLLPath, dwMemSize, NULL))
            {
                if(hUserDLL = LoadLibrary(TEXT("kernel32.dll")))
                {
                    if(lpFuncAddr = GetProcAddress(hUserDLL, "LoadLibraryA"))
                    {
                        if(hThread = CreateRemoteThread(hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)lpFuncAddr, lpBaseAddr, 0, NULL))
                        {
                            WaitForSingleObject(hThread, INFINITE);
                            if(GetExitCodeThread(hThread, &dwExitCode)) {
                                bSuccess = (dwExitCode != 0) ? TRUE : FALSE;
                            }
                            CloseHandle(hThread);
                        }
                    }
                    FreeLibrary(hUserDLL);
                }
            }
            VirtualFreeEx(hProcess, lpBaseAddr, 0, MEM_RELEASE);
        }
        CloseHandle(hProcess);
    }

    return bSuccess;
}

bool InjectDll(char *dllName, DWORD ProcessID)
{
    if (!GimmePrivileges ())
    {
        printf ("Error elivating privileges \n");
        return false;
    }
    else
        printf ("Privileges elivated \n");
    //DWORD ProcessID = GetTargetProcessIdFromProcname (procName); //i didnt include this function because i know for a fact it works
    HANDLE Proc;
    HANDLE RemoteThread;
    char buf[50]={0};
    LPVOID RemoteString, LoadLibAddy;

    if(!ProcessID)
        return false;

    Proc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, ProcessID);

    if(!Proc)
    {
        printf("OpenProcess() failed: %d \n", GetLastError());
        return false;
    }

    LoadLibAddy = (LPVOID)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "LoadLibraryA");

    RemoteString = (LPVOID)VirtualAllocEx(Proc, NULL, strlen(dllName), MEM_RESERVE|MEM_COMMIT, PAGE_READWRITE);
    if (!WriteProcessMemory(Proc, (LPVOID)RemoteString, dllName,strlen(dllName), NULL))
    {
        printf ("writeProcMem error: %d \n", GetLastError ());
        return false;
    }
    RemoteThread = CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLibAddy, (LPVOID)RemoteString, NULL, NULL);  
    if (RemoteThread == NULL)
    {
        printf ("CreateRemoteThread error: %d \n", GetLastError());
        return false;
    }
    CloseHandle(Proc);

    return true;
}