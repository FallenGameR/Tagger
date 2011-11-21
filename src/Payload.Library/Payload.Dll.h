#pragma once

#ifdef HOOKSPY_DLL_EXPORTS
#define HOOKDLL_API __declspec(dllexport)
#else
#define HOOKDLL_API __declspec(dllimport)
#endif

HOOKDLL_API int APIENTRY InjectDll_HookMessageQueue( HWND hWnd, LPSTR lpString );
HOOKDLL_API void APIENTRY InjectDll_CreateRemoteThread( DWORD processId, LPCSTR dllPath );
HOOKDLL_API DWORD APIENTRY FindConhost( DWORD pid );
HOOKDLL_API bool APIENTRY IsConsoleApp( TCHAR* programPath );
HOOKDLL_API void APIENTRY StartHooking( DWORD pid );