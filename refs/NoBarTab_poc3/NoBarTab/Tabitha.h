#pragma once

class Tabitha
{
private:
	ITaskbarList3 *m_pTaskbar;

	std::map< DWORD, int > m_mapProcIdResults;

	std::set< std::wstring, LeoHelpers::LessUpperCase > m_setExeNames;

public:
	Tabitha()
		: m_pTaskbar(NULL)
	{
		RefreshConfig();
	}

	~Tabitha() // Non-virtual
	{
		UnInit();
	}

	bool UnregisterTabs(const std::set< HWND > &setHwndsToProcess)
	{
		if (!Init(true))
		{
			return false;
		}

		for (std::set< HWND >::const_iterator siter = setHwndsToProcess.begin(); siter != setHwndsToProcess.end(); ++siter)
		{
			internalUnregisterTab(*siter);
		}

		UnInit();

		return true;
	}

	void RefreshConfig()
	{
		std::vector< std::wstring > vecExes;
		NoBarTabConfig_Load(vecExes, false);

		m_setExeNames.clear();
		for(std::vector< std::wstring >::const_iterator viter = vecExes.begin(); viter != vecExes.end(); ++viter)
		{
			m_setExeNames.insert(*viter);
		}
	}

private:
	bool Init(bool bReInit)
	{
		if (bReInit)
		{
			UnInit();
		}

		if (m_pTaskbar == NULL)
		{
			if (::CoCreateInstance(CLSID_TaskbarList, NULL, CLSCTX_INPROC_SERVER|CLSCTX_LOCAL_SERVER, __uuidof(ITaskbarList3), reinterpret_cast< void ** >(&m_pTaskbar)) != S_OK
			||	m_pTaskbar == NULL)
			{
				m_pTaskbar = NULL;
				return false;
			}
		}

		return true;
	}

	void UnInit()
	{
		if (m_pTaskbar != NULL)
		{
			m_pTaskbar->Release();
			m_pTaskbar = NULL;
		}

		m_mapProcIdResults.clear();
	}

	bool IsHandledExe(const wchar_t *szExeName)
	{
		return (m_setExeNames.find(szExeName) != m_setExeNames.end());
	}

	bool internalUnregisterTab(HWND hwnd)
	{
		/*
		{
			std::wstring strInfo;

			bool bVisible = ::IsWindowVisible(hwnd) ? true : false;

			wchar_t szClass[1024];

			if (0 == ::GetClassName(hwnd, szClass, _countof(szClass)))
			{
				szClass[0] = L'\0';
			}

			std::wstring strTitle;

			if (!LeoHelpers::GetWindowTextString(hwnd, &strTitle, true))
			{
				strTitle = L"<error or timeout>";
			}

			DWORD dwProcess = 0;
			DWORD dwThread = ::GetWindowThreadProcessId(hwnd, &dwProcess);

			bool bIsDesktop = (hwnd == ::GetDesktopWindow());

			LeoHelpers::StringFormatToString(&strInfo, L"HWND %p%s, ProcId %ld, ThreadId %ld, %s visible, Class \"%s\", Title \"%s\"", hwnd, bIsDesktop?L" (the desktop)":L"", dwProcess, dwThread, bVisible?L"Is":L"Not", szClass, strTitle.c_str());

			LeoHelpers::OutputDebugFormat(L"Tabitha", L"internalUnregisterTab", L"Considering %s", strInfo.c_str());
		}
		*/

		if (::IsWindow(hwnd) && !::IsWindowVisible(hwnd))
		{
			DWORD dwProcId = 0;

			::GetWindowThreadProcessId(hwnd, &dwProcId);

			int &refIdRes = m_mapProcIdResults[dwProcId];

			if (refIdRes == 0)
			{
				refIdRes = -1; // Remember this process as a failure by default.

				HANDLE hProc = ::OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, dwProcId);

				if (hProc != NULL)
				{
					wchar_t szTemp[MAX_PATH];
					DWORD dwNameLen = _countof(szTemp);

					if (::QueryFullProcessImageName(hProc, 0, szTemp, &dwNameLen)
					&&	IsHandledExe(LeoHelpers::GetLastPathPart(szTemp)))
					{
						refIdRes = 1;
					}

					::CloseHandle(hProc);
				}
			}

			if (refIdRes == 1)
			{
			//	LeoHelpers::OutputDebugFormat(L"Tabitha", L"internalUnregisterTab", L"UnregisterTab(%p)", hwnd);
				m_pTaskbar->UnregisterTab(hwnd);
				return true;
			}
		}

		return false;
	}
};
