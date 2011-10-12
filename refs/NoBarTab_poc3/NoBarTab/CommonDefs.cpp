#include "stdafx.h"
#include "CommonDefs.h"

const wchar_t * const g_szMainWindowClass64 = L"NoBarTab_Main_64";
const wchar_t * const g_szMainWindowClass32 = L"NoBarTab_Main_32";

#if defined(_WIN64)
const wchar_t * const g_szMainWindowClass = g_szMainWindowClass64;
#elif defined(_WIN32)
const wchar_t * const g_szMainWindowClass = g_szMainWindowClass32;
#else
#error Neither Win32 nor Win64.
#endif
