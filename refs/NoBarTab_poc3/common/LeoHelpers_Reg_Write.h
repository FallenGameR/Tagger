#pragma once

namespace LeoHelpers
{
	// Returns false on failure. Use GetLastError().
	// hKeyParent -- hKey of parent under which szKeyPath should be created. Usually a hive like HKEY_CURRENT_USER
	// szKeyPath -- Sub key (can be a path) under hKeyHiveOrParent in which szValueName is set. Can be "" or NULL.
	// szValueName -- Name of value to set. Can be "" or NULL to set the unnamed, default value.
	// sam -- Pass zero normally, or KEY_WOW64_32KEY or KEY_WOW64_64KEY if you need to access the alternative area. (KEY_SET_VALUE is specified automatically.)
	// dwType -- Type of data to set. (Same as dwType argument of RegSetValueEx())
	// lpData -- Pointer to data to set. Can be NULL if you just want to create the key but set no values.
	// cbData -- Size of data pointed to by lpData, including null terminator(s) (Same as cbData arg of RegSetValueEx())
	bool RegSetValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam,
							DWORD dwType, const void *lpData, DWORD cbData);

	// Returns false on failure. Use GetLastError().
	// Uses SetValue() to set a DWORD without having to worry about buffers, types and so on.
	bool RegSetDWORDValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, DWORD dwData);

	// Returns false on failure. Use GetLastError().
	// Uses SetValue() to set an int without having to worry about buffers, types and so on.
	bool RegSetIntValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, int iData);

	// Returns false on failure. Use GetLastError().
	// Uses SetValue() to set a bool without having to worry about buffers, types and so on.
	bool RegSetBoolValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, bool bData);

	// Returns false on failure. Use GetLastError().
	// Uses SetValue() to set a String without having to worry about buffers, types and so on.
	bool RegSetStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, const wchar_t *szData);

	// Returns false on failure. Use GetLastError().
	bool RegSetMultiStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, const std::vector< std::wstring > *pVecStrings);

	// Returns false on failure. Use GetLastError().
	// sam -- Pass zero normally, or KEY_WOW64_32KEY or KEY_WOW64_64KEY if you need to access the alternative area.
	bool RegCreateKey(HKEY hKeyParent, const wchar_t *szKeyPath, REGSAM sam);

	// Returns false on failure. Use GetLastError().
	// szKeyPath may be empty/NULL but szKeyToDelete must not be.
	// If bDeleteSubKeys is true then it will do a recursive delete (like SHDeleteKey); else it will fail if there are sub-keys.
	// (Note: I wouldn't trust SHDeleteKey after finding several issues with SHCopyKey. Use this instead.)
	bool RegDeleteKey(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szKeyToDelete, REGSAM sam, bool bDeleteSubKeys);

	// Returns false on failure. Use GetLastError().
	bool RegDeleteValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam);

	// Returns false on failure. Use GetLastError().
	// Recursively copies the specified sub-key from source to destination.
	// Similar to SHCopyKey except that this one works with 64-bit registry virtualisation and Unicode path names.
	// (It turns out SHCopyKey is quite a poor API. :) )
	// Like SHCopyKey, this does not copy permissions.
	// If the destination already exists then the source will be merged into it. i.e. The dest won't be cleaned first.
	// Does not undo its actions if it encounters an error.
	// If an error is encountered it will still try and copy as much as possible before returning the original error.
	bool RegCopyKey(HKEY hKeySourceParent, REGSAM samParent, HKEY hKeyDestParent, REGSAM samDest, const wchar_t *szKeyPath);
};
