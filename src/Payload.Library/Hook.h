#pragma once

#ifdef HOOKSPY_DLL_EXPORTS
#define HOOKDLL_API __declspec(dllexport)
#else
#define HOOKDLL_API __declspec(dllimport)
#endif

HOOKDLL_API void AddDebugPrivilege();
HOOKDLL_API int InjectDll_HookMessageQueue( HWND hWnd, LPSTR lpString );
HOOKDLL_API void InjectDll_CreateRemoteThread( DWORD processId, LPCSTR dllPath );
