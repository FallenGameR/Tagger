#pragma once

// Windows Header Files
#define WIN32_LEAN_AND_MEAN
#include <SDKDDKVer.h>
#include <windows.h>

// C RunTime Header Files
#include <tchar.h>
#include <iostream>
#include <fstream>
#include <sstream> 

#include <tlhelp32.h>
#include <wct.h>
#include <psapi.h>

#if 0

//#include <strsafe.h> 
//#include <stdexcept> 
//#include <streambuf>
//#include <winnt.h>
//#include <string>
//#include <stdio.h>
//#include <stdlib.h>

// Macros

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

#endif


// Using std namespace
using namespace std;
