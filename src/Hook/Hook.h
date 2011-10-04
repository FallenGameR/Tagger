#if !defined HOOKSPY_DLL_H
#define HOOKSPY_DLL_H


#ifdef HOOKSPY_DLL_EXPORTS
#define HOOKDLL_API __declspec(dllexport)
#else
#define HOOKDLL_API __declspec(dllimport)
#endif


HOOKDLL_API void AddDebugPrivilege();
HOOKDLL_API int InjectDll_HookMessageQueue( HWND hWnd, LPSTR lpString );
HOOKDLL_API void InjectDll_CreateRemoteThread( DWORD processId, LPCSTR dllPath );

struct error
{
    char* Message;
    DWORD ErrorCode;

    error( char* message ): Message(message), ErrorCode(GetLastError()) {}
    error( char* message, DWORD lastError): Message(message), ErrorCode(lastError) {}
};

#endif // !defined(HOOKSPY_DLL_H)