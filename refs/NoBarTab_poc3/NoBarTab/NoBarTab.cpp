#include "stdafx.h"
#include "CommonDefs.h"
#include "../common/LeoHelpers_String.h"
#include "../common/LeoHelpers_File.h"
#include "../common/LeoHelpers_Scope.h"
#include "../common/LeoHelpers_Thread.h"
#include "../common/LeoHelpers_Misc.h"
#include "NoBarTab.h"
#include "Tabitha.h"

HINSTANCE g_hInstance = NULL;
HMODULE g_hModDll = NULL;
HOOKPROC g_procDllHookCbt = NULL;
HHOOK g_hHookCbt = NULL;
Tabitha g_Tabitha;
std::map< HWND, DWORD > g_mapWindowTimes;

const UINT_PTR g_timerIdNewWindows = 1;
const UINT g_uiTimerDuration = 750;
const DWORD g_dwMinWindowAge = 500;


#if defined(_WIN64)
const wchar_t * const g_szDllName = L"NoBarTabDll64.dll";
#elif defined(_WIN32)
const wchar_t * const g_szDllName = L"NoBarTabDll32.dll";
#else
#error Neither Win32 nor Win64.
#endif

const wchar_t * const g_szRunArg = L"/run";
const wchar_t * const g_szStopArg = L"/stop";
const wchar_t * const g_szCleanArg = L"/clean";

int InitInstance(const wchar_t *szCmdLine);
LRESULT CALLBACK MainWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
bool GetExePath(std::wstring &strPath, bool bOtherBitness);
bool LaunchExe(bool bOtherBitness, const wchar_t *szArgs);
bool GetInstallCommand(std::wstring &strCmd);

int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// Plug "binary planting" security hole by removing the current-directory from the DLL search path. (Does NOT affect the program directory.)
	::SetDllDirectory(L"");

	MSG msg = {0};

	g_hInstance = hInstance;

	if (::CoInitialize(NULL) != S_OK)
	{
		::MessageBox(NULL, L"Failed to initialize COM", L"NoBarTab", MB_ICONERROR|MB_OK);
	}
	else
	{
		g_hModDll = ::LoadLibrary(g_szDllName);

		if (g_hModDll == NULL)
		{
			::MessageBox(NULL, L"Failed to load hook DLL", L"NoBarTab", MB_ICONERROR|MB_OK);
		}
		else
		{
			g_procDllHookCbt = reinterpret_cast< HOOKPROC >( ::GetProcAddress(g_hModDll, "NoBarTab_CBTProc") );

			if (g_procDllHookCbt == NULL)
			{
				::MessageBox(NULL, L"Failed to find required export in hook DLL", L"NoBarTab", MB_ICONERROR|MB_OK);
			}
			else
			{
				int initRes = InitInstance(lpCmdLine);

				if (initRes == 0) // Failure
				{
					::MessageBox(NULL, L"Failed to initialize", L"NoBarTab", MB_ICONERROR|MB_OK);
				}
				else if (initRes == 1) // Success, and run. (The third possibility is that InitInstance sent a message to another instance and we should now exit immediately.)
				{
					while(true)
					{
						BOOL bMsg = ::GetMessage(&msg, NULL, 0, 0);
						if (bMsg == 0 || bMsg == -1)
						{
							break;
						}

					//	if (!::TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
						{
							::TranslateMessage(&msg);
							::DispatchMessage(&msg);
						}
					}
				}
			}

			::FreeLibrary(g_hModDll);
			g_hModDll = NULL;
		}

		::CoUninitialize();
	}

	return static_cast< int >( msg.wParam );
}

int InitInstance(const wchar_t *szCmdLine)
{
	bool bStop    = false;
	bool bClean   = false;
	bool bRunOnly = false;

	std::vector< std::wstring > vecCmdWords;

	if (LeoHelpers::Tokenize(szCmdLine, L" ", NULL, &vecCmdWords, true, true, false))
	{
		for(std::vector< std::wstring >::const_iterator viter = vecCmdWords.begin(); viter != vecCmdWords.end(); ++viter)
		{
			if (0 == LeoHelpers::WideStrCmpUpper(viter->c_str(), g_szStopArg) ) { bStop    = true; }
			if (0 == LeoHelpers::WideStrCmpUpper(viter->c_str(), g_szCleanArg)) { bClean   = true; }
			if (0 == LeoHelpers::WideStrCmpUpper(viter->c_str(), g_szRunArg)  ) { bRunOnly = true; }
		}
	}

	if (bStop || bClean)
	{
		HWND hWndExisting64 = ::FindWindow(g_szMainWindowClass64, g_szMainWindowClass64);
		HWND hWndExisting32 = ::FindWindow(g_szMainWindowClass32, g_szMainWindowClass32);
		if (hWndExisting64 != NULL) { ::PostMessage(hWndExisting64, WM_NOBARTAB_STOP, 0, 0); }
		if (hWndExisting32 != NULL) { ::PostMessage(hWndExisting32, WM_NOBARTAB_STOP, 0, 0); }

		if (bClean)
		{
			NoBarTabConfig_Wipe();
		}

		return -1; // Tell caller to exit without reporting any (further) failure messages.
	}

	// On 64-bit Windows, 32-bit version always ensures 64-bit version is running by launching 64-bit exe with /run.
	// (If 64-bit version already running then the new copy will simply exit.)
	// The aim is to make it so people (and registry settings etc.) just run run 32-bit exe.
	if (LeoHelpers::Is32BitProcessOn64BitWindows())
	{
		LaunchExe(true, g_szRunArg);
	}

	LeoHelpers::MutexWrapper mx(g_szMainWindowClass32);
	LeoHelpers::MutexScoper mxs(mx);

	HWND hWndExisting = ::FindWindow(g_szMainWindowClass, g_szMainWindowClass);

	if (bRunOnly)
	{
		// Either bitness version run with /run; if existing instance, just exit, else we'll start one.
		if (hWndExisting != NULL)
		{
			return -1; // Tell caller to exit without reporting any (further) failure messages.
		}
	}
	else if (LeoHelpers::Is64BitProcess())
	{
		// 64-bit version run with no args: Launch 32-bit version w/o args, then exit.
		// (32-bit version will launch, re-launch 64-bit version with "/run", then open its own config dialog.)
		// This is handled in case someone manually double-clicks the 64-bit exe.
		mxs.ReleaseNow(); // Release mutex early as LaunchExe may display an error dialog.
		LaunchExe(true, NULL);
		return -1; // Tell caller to exit without reporting any (further) failure messages.
	}

	HWND hWndMain = hWndExisting;

	if (hWndMain == NULL)
	{
		WNDCLASSEX wcex = {0};
		wcex.cbSize = sizeof(wcex);
		wcex.style			= CS_HREDRAW | CS_VREDRAW;
		wcex.lpfnWndProc	= MainWndProc;
		wcex.cbClsExtra		= 0;
		wcex.cbWndExtra		= 0;
		wcex.hInstance		= g_hInstance;
		wcex.hIcon			= ::LoadIcon(g_hInstance, MAKEINTRESOURCE(IDI_NOBARTAB));
		wcex.hCursor		= ::LoadCursor(NULL, IDC_ARROW);
		wcex.hbrBackground	= reinterpret_cast< HBRUSH >( COLOR_WINDOW+1 );
		wcex.lpszMenuName	= NULL;
		wcex.lpszClassName	= g_szMainWindowClass;
		wcex.hIconSm		= ::LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

		if (::RegisterClassEx(&wcex) == 0)
		{
			return 0; // Failure
		}

		hWndMain = ::CreateWindow(g_szMainWindowClass, g_szMainWindowClass, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, NULL, NULL, g_hInstance, NULL);

		if (hWndMain == NULL)
		{
			return 0; // Failure
		}

		::ShowWindow(hWndMain, SW_SHOW);
		::UpdateWindow(hWndMain);
	}

	if (!bRunOnly && LeoHelpers::Is32BitProcess())
	{
		::PostMessage(hWndMain, WM_NOBARTAB_SHOWCONFIG, 0, 0);
	}

	if (hWndExisting == NULL)
	{
		return 1; // Success, and we should run a message loop as we created the main window.
	}
	else
	{
		return -1; // Tell caller to exit without reporting any (further) failure messages.
	}
}

BOOL CALLBACK AllWindowProc(HWND hwnd, LPARAM lParam)
{
	g_mapWindowTimes[hwnd] = ::GetTickCount();

	return TRUE;
}

bool FilterAllWindows(HWND hWnd)
{
#if !defined(_WIN64)
	g_mapWindowTimes.clear();
	// Put all windows into the map to be processed in a moment.
	// (Not processed immediately in case they are being created at the same time as us and are not visible yet.)
	// This is only done by the 32-bit executable, so on 64-bit systems where both are launched they don't both do it.
	if (!::EnumWindows(AllWindowProc, 0))
	{
		return false;
	}
	::SetTimer(hWnd, g_timerIdNewWindows, g_uiTimerDuration, NULL);
#endif
	return true;
}

LRESULT CALLBACK MainWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	//case WM_COMMAND:
	//	switch (LOWORD(wParam)) // wmEvent = HIWORD(wParam);
	//	{
	//	default:
	//		break;
	//	}
	//	break;

	case WM_PAINT:
		{
			PAINTSTRUCT ps;
			HDC hdc = ::BeginPaint(hWnd, &ps);
			if (hdc != NULL)
			{
				::EndPaint(hWnd, &ps);
			}
		}
		return 0;

	case WM_CREATE:
		if (!::ChangeWindowMessageFilterEx(hWnd, WM_NOBARTAB_WINDOWCREATED_CBT, MSGFLT_ALLOW, NULL))
		{
			return -1; // destroy the window instead.
		}
		if (g_hHookCbt == NULL)
		{
			g_hHookCbt = ::SetWindowsHookEx(WH_CBT, g_procDllHookCbt, g_hModDll, 0);
		}
		if (g_hHookCbt == NULL)
		{
			return -1; // destroy the window instead.
		}
		if (!FilterAllWindows(hWnd))
		{
			return -1; // destroy the window instead.
		}
		break;

	case WM_DESTROY:
		if (g_hHookCbt != NULL)
		{
			::UnhookWindowsHookEx(g_hHookCbt);
			g_hHookCbt = NULL;
		}
		::PostQuitMessage(0);
		break;

	case WM_NOBARTAB_STOP:
		::DestroyWindow(hWnd);
		return 0;

	case WM_NOBARTAB_WINDOWCREATED_CBT:
		{
			HWND hwndCreated = reinterpret_cast< HWND >( wParam );

		//	LeoHelpers::OutputDebugFormat(L"Tabitha", L"WM_NOBARTAB_WINDOWCREATED_CBT", L"hwndCreated = %p", hwndCreated);

			bool bMapWasEmpty = g_mapWindowTimes.empty();

			g_mapWindowTimes[hwndCreated] = ::GetTickCount();

			if (bMapWasEmpty)
			{
				// Timer won't be running already so we need to start it up.
				::SetTimer(hWnd, g_timerIdNewWindows, g_uiTimerDuration, NULL);
			}
		}
		return 0;

	case WM_TIMER:
		if (wParam == g_timerIdNewWindows)
		{
			// Build them into a local set, just in case it's not safe to do the actual clean-up inside of the
			// loop through g_mapWindowTimes (e.g. in case calling the COM method puts us into a message loop
			// that allows WM_NOBARTAB_WINDOWCREATED_CBT to be processed which modifies g_mapWindowTimes and
			// messes up our iterators.) This also lets the Tabitha object do a single init/uninit for the lot.
			std::set< HWND > setHwndsToProcess;
			DWORD dwNow = ::GetTickCount();

			for (std::map< HWND, DWORD >::iterator miter = g_mapWindowTimes.begin(); miter != g_mapWindowTimes.end();)
			{
				DWORD dwAge = (dwNow - miter->second);

				if (dwAge < g_dwMinWindowAge)
				{
				//	LeoHelpers::OutputDebugFormat(L"Tabitha", L"WM_TIMER", L"Leaving = %p (%lums)", miter->first, dwAge);
					++miter;
				}
				else
				{
				//	LeoHelpers::OutputDebugFormat(L"Tabitha", L"WM_TIMER", L"Doing = %p (%lums)", miter->first, dwAge);
					setHwndsToProcess.insert(miter->first);
					miter = g_mapWindowTimes.erase(miter);
				}
			}

			if (g_mapWindowTimes.empty())
			{
				// No pending windows left so stop the timer.
				::KillTimer(hWnd, g_timerIdNewWindows);
			}

			if (!setHwndsToProcess.empty())
			{
				g_Tabitha.UnregisterTabs(setHwndsToProcess);
			}
		}
		break;

	case WM_NOBARTAB_SHOWCONFIG:
		MessageBox(NULL, L"Config not done yet.", L"NoBarTab", MB_OK);
		return 0;

	case WM_NOBARTAB_REFRESHCONFIG:
		g_Tabitha.RefreshConfig();
		FilterAllWindows(hWnd);
		return 0;

	default:
		break;
	}

	return ::DefWindowProc(hWnd, message, wParam, lParam);
}

bool GetExePath(std::wstring &strPath, bool bOtherBitness)
{
	wchar_t szPath[MAX_PATH];
	DWORD dwRes = ::GetModuleFileName(NULL, szPath, _countof(szPath));
	if (dwRes == 0 || dwRes >= _countof(szPath))
	{
		return false;
	}

	if (bOtherBitness)
	{
		wchar_t *szName = LeoHelpers::GetLastPathPart(szPath);

#if defined(_WIN64)
		wchar_t *szChange = wcsstr(szName, L"64");
		if (szChange == NULL) { return false; }
		szChange[0] = L'3';
		szChange[1] = L'2';
#elif defined(_WIN32)
		wchar_t *szChange = wcsstr(szName, L"32");
		if (szChange == NULL) { return false; }
		szChange[0] = L'6';
		szChange[1] = L'4';
#else
#error Neither Win32 nor Win64.
#endif
	}

	strPath = szPath;
	return true;
}

bool LaunchExe(bool bOtherBitness, const wchar_t *szArgs)
{
	std::wstring strPath;
	if (!GetExePath(strPath, bOtherBitness))
	{
		::MessageBox(NULL, L"Failed to get exe path", L"NoBarTab", MB_ICONERROR|MB_OK);
		return false;
	}

	if (!LeoHelpers::CreateProcessWrapper(strPath.c_str(), szArgs, NULL, true, NULL))
	{
		std::wstring strMsg;
		LeoHelpers::StringFormatToString(&strMsg, L"Failed to launch \"%s\"", strPath.c_str());
		::MessageBox(NULL, strMsg.c_str(), L"NoBarTab", MB_ICONERROR|MB_OK);
		return false;
	}

	return true;
}

bool GetInstallCommand(std::wstring &strCmd)
{
	strCmd.clear();

	std::wstring strPath;
	if (!GetExePath(strPath, LeoHelpers::Is64BitProcess())) // Always get 32-bit process path. (If 64-bit we ask for the other; if not we ask for the current.)
	{
		::MessageBox(NULL, L"Failed to get exe path", L"NoBarTab", MB_ICONERROR|MB_OK);
		return false;
	}

	strCmd = L"\"";
	strCmd += strPath;
	strCmd += L"\" ";
	strCmd += g_szRunArg;

	return true;
}
