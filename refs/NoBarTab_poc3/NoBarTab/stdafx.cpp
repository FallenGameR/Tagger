// stdafx.cpp : source file that includes just the standard includes
// NoBarTab.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"
#include "../common/LeoHelpers_String.h"
#include "../common/LeoHelpers_Misc.h"
#include "../common/LeoHelpers_Reg_Read.h"
#include "../common/LeoHelpers_Reg_Write.h"

const wchar_t * const g_szConRegKey     = L"Software\\PretentiousName\\NoBarTab";
const wchar_t * const g_szConRegValExes = L"Exes";

const wchar_t * const g_szConRegRunKey  = L"Software\\Microsoft\\Windows\\CurrentVersion\\Run";
const wchar_t * const g_szConRegRunVal  = L"PN_NoBarTab";

bool NoBarTabConfig_Load(std::vector< std::wstring > &vecExes, bool bDefaultIfNotInRegistry)
{
	REGSAM sam = (LeoHelpers::Is64BitWindows() ? KEY_WOW64_64KEY : 0);

	if (!LeoHelpers::RegQueryMultiStringValue(HKEY_CURRENT_USER, g_szConRegKey, g_szConRegValExes, sam, &vecExes))
	{
		vecExes.clear();
		return false;
	}

	return true;
}

bool NoBarTabConfig_Save(const std::vector< std::wstring > &vecExes)
{
	REGSAM sam = (LeoHelpers::Is64BitWindows() ? KEY_WOW64_64KEY : 0);

	return LeoHelpers::RegSetMultiStringValue(HKEY_CURRENT_USER, g_szConRegKey, g_szConRegValExes, sam, &vecExes);
}

bool NoBarTabConfig_Install(const wchar_t *szCmd)
{
	REGSAM sam = (LeoHelpers::Is64BitWindows() ? KEY_WOW64_64KEY : 0);

	return LeoHelpers::RegSetStringValue(HKEY_CURRENT_USER, g_szConRegRunKey, g_szConRegRunVal, sam, szCmd);
}

bool NoBarTabConfig_IsInstalled(const wchar_t *szCmd)
{
	REGSAM sam = (LeoHelpers::Is64BitWindows() ? KEY_WOW64_64KEY : 0);

	std::wstring strVal;

	if (!LeoHelpers::RegQueryStringValue(HKEY_CURRENT_USER, g_szConRegRunKey, g_szConRegRunVal, sam, &strVal))
	{
		return false;
	}

	return (0 == LeoHelpers::WideStrCmpUpper(strVal.c_str(), szCmd));
}

bool NoBarTabConfig_Wipe()
{
	REGSAM sam = (LeoHelpers::Is64BitWindows() ? KEY_WOW64_64KEY : 0);

	bool bResult = true;

	if (!LeoHelpers::RegDeleteKey(HKEY_CURRENT_USER, NULL, g_szConRegKey, sam, true))            { bResult = false; }
	if (!LeoHelpers::RegDeleteValue(HKEY_CURRENT_USER, g_szConRegRunKey, g_szConRegRunVal, sam)) { bResult = false; }

	return bResult;
}
