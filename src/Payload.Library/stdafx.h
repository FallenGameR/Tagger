#pragma once

// Windows Header Files
#define WIN32_LEAN_AND_MEAN
#include <SDKDDKVer.h>
#include <windows.h>

// C RunTime Header Files
#include <tchar.h>
#include <strsafe.h> 
#include <stdexcept> 
#include <sstream> 

// Other includes
#include <winnt.h>
#include <tlhelp32.h>
#include <iostream>	
#include <string>
#include <wct.h>
#include <psapi.h>
#include <stdio.h>
#include <stdlib.h>

// Using std namespace
using namespace std;

// Macros

// Types

// Variables
PROCESS_INFORMATION hConsole1;
PROCESS_INFORMATION hTargetConsole;


// Forwards


//----------------------
// Another Spy
// Constants
TCHAR szTitle[] = TEXT("Windows Target x86");
TCHAR szWindowClass[] = TEXT("TARGET");

// Global Variables
HWINEVENTHOOK hWinEventHook;

// Forwards
void Initialization( DWORD processId );
void CleanupSpy();
void CreateTargetConsoleWindow();
void CALLBACK WinEventProcSpy( HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime );
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );


//----------------------
// Console determiner


VOID  main(int, char **);
DWORD AbsoluteSeek(HANDLE, DWORD);
VOID  ReadBytes(HANDLE, LPVOID, DWORD);
VOID  WriteBytes(HANDLE, LPVOID, DWORD);
VOID  CopySection(HANDLE, HANDLE, DWORD);

// http://support.microsoft.com/kb/90493
#define XFER_BUFFER_SIZE 2048

// http://blogs.msdn.com/b/jeremykuhne/archive/2008/02/19/sscli-2-0-and-visual-studio-2008.aspx
#pragma warning (disable :4985)

#define IMAGE_SIZEOF_NT_OPTIONAL32_HEADER    224
#define IMAGE_SIZEOF_NT_OPTIONAL64_HEADER    240

//sizeof(IMAGE_OPTIONAL_HEADER)
#ifdef _WIN64
#define IMAGE_SIZEOF_NT_OPTIONAL_HEADER     IMAGE_SIZEOF_NT_OPTIONAL64_HEADER
#else
#define IMAGE_SIZEOF_NT_OPTIONAL_HEADER     IMAGE_SIZEOF_NT_OPTIONAL32_HEADER
#endif


//----------------------
// Enumerating processes

//----------------------
// Using WCT
