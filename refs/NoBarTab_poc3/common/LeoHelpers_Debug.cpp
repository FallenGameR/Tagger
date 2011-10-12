#include "stdafx.h"
#include "LeoHelpers_String.h"
#include "LeoHelpers_Scope.h"
#include "LeoHelpers_File.h"
#include "LeoHelpers_Debug.h"

void LeoHelpers::OutputDebug(const wchar_t *szPrefix, const wchar_t *szClass, const wchar_t *szMethod, const wchar_t *szMessage)
{
	wchar_t szThreadId[32];

	DWORD dwThreadId = GetCurrentThreadId();
	_snwprintf_s(szThreadId, _countof(szThreadId), _TRUNCATE, L"[Thread %ld] ", dwThreadId);

	std::wstring strMsg = szThreadId;

	if (szPrefix != NULL)
	{
		strMsg += szPrefix;
	}

	if (szClass != NULL)
	{
		strMsg += szClass;

		if (szMethod != NULL)
		{
			strMsg += L"::";
		}
		else
		{
			strMsg += L": ";
		}
	}

	if (szMethod != NULL)
	{
		strMsg += szMethod;
		strMsg += L": ";
	}

	if (szMessage != NULL)
	{
		strMsg += szMessage;
	}

	while(!strMsg.empty() && iswspace(strMsg[strMsg.length() - 1]))
	{
		strMsg.resize(strMsg.length() - 1);
	}

	strMsg += L"\r\n";

	::OutputDebugString(strMsg.c_str());
}


#ifdef _DEBUG

// static
unsigned __stdcall LeoHelpers::LeoAssert::staticThread(void *pVoidThis)
{
	LeoAssert &la = *reinterpret_cast< LeoAssert * >(pVoidThis);

	if (S_OK != ::CoInitialize(NULL))
	{
		return 0;
	}

	std::wstring strMsg;
	LeoHelpers::StringFormatToString(&strMsg, L"DEBUG ASSERTION FAILED\n\nCondition: %s\n\nFile: %s\nLine: %d\n\nBreak?", la.m_szExpression, la.m_szFile, la.m_iLine);

	wchar_t szModulePath[MAX_PATH+1] = {0};
	GetModuleFileName(NULL, szModulePath, _countof(szModulePath)-1);
	szModulePath[_countof(szModulePath)-1] = L'\0';

	if (IDYES == ::MessageBox(NULL, strMsg.c_str(), LeoHelpers::GetLastPathPart(szModulePath), MB_YESNO|MB_ICONEXCLAMATION|MB_TOPMOST))
	{
		la.m_bBreak = true;
	}

	::CoUninitialize();

	return 0;
}

//static
void LeoHelpers::LeoAssert::Show(const wchar_t *szExpression, const wchar_t *szFile, int iLine)
{
	LeoAssert la(szExpression, szFile, iLine);

	unsigned int uiIgnored = 0;

	HANDLE hThread = reinterpret_cast<HANDLE>( ::_beginthreadex(NULL, 0, staticThread, &la, 0, &uiIgnored) );

	if (hThread != NULL)
	{
		::WaitForSingleObject(hThread, INFINITE);
		::CloseHandle(hThread);
	}

	if (la.m_bBreak)
	{
		::_CrtDbgBreak();
	}
}

#endif
