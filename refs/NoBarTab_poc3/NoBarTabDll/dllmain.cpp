#include "stdafx.h"
#include "../NoBarTab/CommonDefs.h"

BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReasonForCall, LPVOID lpReserved)
{
	switch (dwReasonForCall)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

extern "C" LRESULT CALLBACK NoBarTab_CBTProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode == HCBT_CREATEWND && wParam != 0 && lParam != 0)
	{
		CBT_CREATEWND *pCreateParams = reinterpret_cast< CBT_CREATEWND * >( lParam );

		// Only care about top-level windows.
		if (pCreateParams->lpcs != NULL && pCreateParams->lpcs->hwndParent == NULL)
		{
			HWND hWndMain = ::FindWindow(g_szMainWindowClass, g_szMainWindowClass);

			if (hWndMain != NULL)
			{
				::PostMessage(hWndMain, WM_NOBARTAB_WINDOWCREATED_CBT, wParam, 0);
			}
		}
	}

	return ::CallNextHookEx(NULL, nCode, wParam, lParam);
}
