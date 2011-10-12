#pragma once

namespace LeoHelpers
{
	// Returns false on failure. Use GetLastError().
	// hKeyParent -- hKey of parent under which szKeyPath should be created. Usually a hive like HKEY_CURRENT_USER
	// szKeyPath -- Sub key (can be a path) under hKeyHiveOrParent from which szValueName is read. Can be "" or NULL.
	// szValueName -- Name of value to query. Can be "" or NULL to read the unnamed default value.
	// sam -- Pass zero normally, or KEY_WOW64_32KEY or KEY_WOW64_64KEY if you need to access the alternative area. (KEY_QUERY_VALUE is specified automatically.)
	// pdwType -- Pointer to DWORD to receive type of data. NULL if unwanted.
	// plpData -- POINTER TO POINTER to receive data buffer. NULL if data unwanted.
	// pcbData -- Pointer to DWORD to receive size of data buffer. NULL if data unwanted.
	// You do NOT allocate a buffer, this function does it for you (like the stupid Registry API should have done, at least as an option).
	// If the function succeeds and plpData was not NULL you must delete[] *plpData when finished with it.
	// Like RegQueryValueEx, you may supply NULL plpData and non-NULL pcbData to get the required
	// buffer size (except for HKEY_PERFORMANCE_DATA), but again the function always allocates a buffer for you.
	// Although untested, reading HKEY_PERFORMANCE_DATA should work.
	// Pads whatever data comes out of the registry with 8 null bytes to avoid problems with badly stored data.
	bool RegQueryValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, DWORD *pdwType, void **plpData, DWORD *pcbData);

	// Returns false on failure. Use GetLastError().
	// Uses QueryValue() to get you a DWORD without having to worry about buffers, types and so on.
	bool RegQueryDWORDValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, DWORD *pdwRes);

	// Returns false on failure. Use GetLastError().
	// Uses QueryValue() to get you an int without having to worry about buffers, types and so on.
	bool RegQueryIntValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, int *piRes);

	// Returns false on failure. Use GetLastError().
	// Uses QueryValue() to get you a bool without having to worry about buffers, types and so on.
	bool RegQueryBoolValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, bool *pBool);

	// Returns false on failure. Use GetLastError().
	// Uses QueryValue() to get you a wchar_t string without having to worry about buffers, types and so on.
	// This will only succeed with REG_SZ values. If you need REG_EXPAND_SZ use QueryExpandStringValue.
	bool RegQueryStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, std::wstring *pString);

	// Returns false on failure. Use GetLastError().
	// Uses QueryValue() to get you a wchar_t string without having to worry about buffers, types and so on.
	// If the value type is REG_SZ then the value is returned unaltered.
	// If the value type is REG_EXPAND_SZ then the bExpand arg determines whether it is expanded or returned unaltered.
	bool RegQueryExpandStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, bool bExpand, REGSAM sam, std::wstring *pString);

	// Returns false on failure. Use GetLastError().
	// Uses QueryValue() to get you a vector of wchar_t strings without having to worry about buffers, types and so on.
	bool RegQueryMultiStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, std::vector< std::wstring > *pVecStrings);

	// Returns false on failure. Use GetLastError().
	// Returns vector of child key names.
	// If szPrefixFilter is non-NULL then only key names starting with it will be included, and it will be removed from the start of them.
	bool RegEnumKey(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szPrefixFilter, REGSAM sam, std::vector< std::wstring > *pVecStrings);

	// Returns false on failure. Use GetLastError().
	// Returns vector of value names.
	// KEY_QUERY_VALUE will automatically be specified as part of the access mask.
	// The sam argument allows you to pass KEY_WOW64_32KEY or KEY_WOW64_64KEY; pass zero otherwise.
	bool RegEnumValue(HKEY hKeyParent, const wchar_t *szKeyPath, REGSAM sam, std::vector< std::wstring > *pVecStrings);

	// Returns false on failure (including if the key does not exist). Use GetLastError().
	// The sam argument allow you to pass KEY_WOW64_32KEY or KEY_WOW64_64KEY; pass zero otherwise.
	bool RegDoesKeyExist(HKEY hKeyParent, const wchar_t *szKeyPath, REGSAM sam);

	// Returns false on failure. Use GetLastError().
	bool ExpandEnvVars(std::wstring *pstrOut, const wchar_t *szInput);

	// Returns false on failure. Use GetLastError().
	// If the string isn't in the @blah.dll,-1234 format then the function returns true and does not modify the string.
	// Otherwise, the function tries to look up the string in the given DLL and returns boolean success.
	// If the string contains env-vars like %SystemRoot% then you should first expand them via ExpandEnvVars.
	bool ConvertRegistryStyleLocalisedString(std::wstring &str);
};
