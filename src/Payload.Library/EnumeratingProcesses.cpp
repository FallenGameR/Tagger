#include "stdafx.h"

#if 0

#include <windows.h>
#include <tlhelp32.h>
#include <iostream>	
#include <string>

using namespace std;

int main( )
{
    cout<<endl<<"Running Processes"<<endl;
    HANDLE hSnapShot=CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS,0);
    PROCESSENTRY32* processInfo=new PROCESSENTRY32;
    processInfo->dwSize=sizeof(PROCESSENTRY32);
    int index=0;

    while(Process32Next(hSnapShot,processInfo)!=FALSE)
    {
    cout<<endl<<"***********************************************";	
    cout<<endl<<"\t\t\t"<<++index;
    cout<<endl<<"***********************************************";	
    cout<<endl<<"Parent Process ID: "<<processInfo->th32ParentProcessID;
    cout<<endl<<"Process ID: "<<processInfo->th32ProcessID;
    cout<<endl<<"Name: "<<processInfo->szExeFile;
    cout<<endl<<"Current Threads: "<<processInfo->cntThreads;
    cout<<endl<<"Current Usage: "<<processInfo->cntUsage;
    cout<<endl<<"Flags: "<<processInfo->dwFlags;
    cout<<endl<<"Size: "<<processInfo->dwSize;
    cout<<endl<<"Primary Class Base: "<<processInfo->pcPriClassBase;
    cout<<endl<<"Default Heap ID: "<<processInfo->th32DefaultHeapID;
    cout<<endl<<"Module ID: "<<processInfo->th32ModuleID;
    }

    CloseHandle(hSnapShot);
    cout<<endl;
    cout<<endl<<"***********************************************";
    cout<<endl<<endl;
}

#endif