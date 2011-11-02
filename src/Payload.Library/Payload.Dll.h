#pragma once

#ifdef HOOKSPY_DLL_EXPORTS
#define HOOKDLL_API __declspec(dllexport)
#else
#define HOOKDLL_API __declspec(dllimport)
#endif

HOOKDLL_API void APIENTRY AddDebugPrivilege();
HOOKDLL_API int APIENTRY InjectDll_HookMessageQueue( HWND hWnd, LPSTR lpString );
HOOKDLL_API void APIENTRY InjectDll_CreateRemoteThread( DWORD processId, LPCSTR dllPath );
HOOKDLL_API int _cdecl UsingWctMain ( __in int argc, __in_ecount(argc) PWSTR* argv);

// http://www.codeproject.com/KB/cpp/howto_export_cpp_classes.aspx
// Very good article regarding consequences of exporting class in dll
