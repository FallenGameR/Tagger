#if !defined HOOKSPY_DLL_H
#define HOOKSPY_DLL_H


#ifdef HOOKSPY_DLL_EXPORTS
#define HOOKDLL_API __declspec(dllexport)
#else
#define HOOKDLL_API __declspec(dllimport)
#endif


HOOKDLL_API int GetWindowTextRemote(HWND hWnd, LPSTR lpString);


#endif // !defined(HOOKSPY_DLL_H)