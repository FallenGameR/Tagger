#include "stdafx.h"
#include "LeoHelpers_Reg_Read.h"

bool LeoHelpers::RegQueryValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam,
									   DWORD *pdwType, void **plpData, DWORD *pcbData)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	HKEY hKeyToReadValueFrom;

	DWORD dwPcdDataStandin; // Used for getting the registry size if the user doesn't care about it.

	if (NULL == pcbData)
	{
		pcbData = &dwPcdDataStandin;
	}

	// szKeyPath may be NULL in which case another handle like hKeyParent is openned (and must be closed).
	regRes = ::RegOpenKeyEx(hKeyParent, szKeyPath, 0, sam | KEY_QUERY_VALUE, &hKeyToReadValueFrom);

	if (ERROR_SUCCESS == regRes)
	{
		if (NULL == plpData)
		{
			// They just want the type and/or the data size.
			regRes = ::RegQueryValueEx(hKeyToReadValueFrom, szValueName, 0, pdwType, NULL, pcbData);
		}
		else
		{
			// We add 8 bytes of zero padding to the end of any buffer we return. This is because
			// the registry does not check inputs to ensure they are the correct length for the
			// datatype and also does not ensure that strings are null-terminated and multi-strings are
			// double-null-terminated. Rather than add checks into all functions which call this one,
			// we pad with as many zero bytes as are needed to correct anything we might read.
			// 8 means a QUADDWORD that was zero length in the registry would still be safe.
			// 8 is also more than enough for double-null-terminator of a Unicode multi-string.
			const DWORD dwZeroPadding = 8;

			BYTE *allocatedBuffer = NULL;
			DWORD dwAllocatedBufferSize = 0; // For HKEY_PERFORMANCE_DATA, really.

			// Get the required buffer size by giving NULL for data buffer.
			regRes = ::RegQueryValueEx(hKeyToReadValueFrom, szValueName, 0, pdwType, NULL, pcbData);

			if (ERROR_SUCCESS == regRes)
			{
				dwAllocatedBufferSize = *pcbData;
				allocatedBuffer = new BYTE[dwAllocatedBufferSize + dwZeroPadding];
			}
			else if (ERROR_MORE_DATA == regRes)
			{
				// Most likely tried to read HKEY_PERFORMANCE_DATA. *pcdData is undefined at this point.
				*pcbData = 1024; // Must be a number x such that (Int(x/2) > 0)
				dwAllocatedBufferSize = *pcbData;
				allocatedBuffer = new BYTE[dwAllocatedBufferSize + dwZeroPadding];
			}
			else
			{
				*pcbData = 0;
				// leave allocatedBuffer NULL so the while loop does not execute.
			}

			while (NULL != allocatedBuffer)
			{
				regRes = ::RegQueryValueEx(hKeyToReadValueFrom, szValueName, 0, pdwType, allocatedBuffer, pcbData);

				if (ERROR_SUCCESS == regRes)
				{
					// Zero out the padding we added.
					ZeroMemory(allocatedBuffer + dwAllocatedBufferSize, dwZeroPadding);

					*plpData = allocatedBuffer;
					allocatedBuffer = NULL;
					break;
				}
				else if (ERROR_MORE_DATA == regRes)
				{
					// Try a larger buffer.
					delete [] allocatedBuffer;
					*pcbData = dwAllocatedBufferSize + (dwAllocatedBufferSize/2);
					dwAllocatedBufferSize = *pcbData;
					allocatedBuffer = new BYTE[dwAllocatedBufferSize + dwZeroPadding];
				}
				else
				{
					*pcbData = 0;
					delete [] allocatedBuffer;
					allocatedBuffer = NULL;
					break;
				}
			}
		}

		LONG closeRes = ::RegCloseKey(hKeyToReadValueFrom);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::RegQueryDWORDValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, DWORD *pdwRes)
{
	DWORD dwErrToSet = ERROR_SUCCESS;

	DWORD dwType;
	void *pData;

	if (NULL != pdwRes)
	{
		*pdwRes = 0;
	}

	DWORD cbData = 0;

	if (! LeoHelpers::RegQueryValue(hKeyParent, szKeyPath, szValueName, sam, &dwType, &pData, &cbData) )
	{
		dwErrToSet = GetLastError();
	}
	else
	{
		if (REG_DWORD != dwType)
		{
			dwErrToSet = ERROR_INVALID_DATA;
		}
		else
		{
			if (NULL != pdwRes)
			{
				*pdwRes = *((DWORD*)pData);
			}
		}

		delete [] pData;
	}

	if (ERROR_SUCCESS != dwErrToSet)
	{
		::SetLastError(dwErrToSet);
	}

	return(ERROR_SUCCESS == dwErrToSet);
}

bool LeoHelpers::RegQueryIntValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, int *pInt)
{
	DWORD dwData = 0;

	if (!LeoHelpers::RegQueryDWORDValue(hKeyParent, szKeyPath, szValueName, sam, &dwData))
	{
		return false;
	}

	assert(sizeof(int)==sizeof(DWORD));

	if (NULL != pInt && sizeof(int)==sizeof(DWORD))
	{
		*pInt = *reinterpret_cast<int*>(&dwData);
	}

	return true;
}

bool LeoHelpers::RegQueryBoolValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, bool *pBool)
{
	DWORD dwData = 0;

	if (!LeoHelpers::RegQueryDWORDValue(hKeyParent, szKeyPath, szValueName, sam, &dwData))
	{
		return false;
	}

	if (NULL != pBool)
	{
		*pBool = (dwData ? true : false);
	}

	return true;
}

bool LeoHelpers::RegQueryStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, std::wstring *pString)
{
	DWORD dwErrToSet = ERROR_SUCCESS;

	DWORD dwType;
	void *pData;

	pString->erase();

	if (! LeoHelpers::RegQueryValue(hKeyParent, szKeyPath, szValueName, sam, &dwType, &pData, NULL) )
	{
		dwErrToSet = GetLastError();
	}
	else
	{
		if (REG_SZ != dwType)
		{
			dwErrToSet = ERROR_INVALID_DATA;
		}
		else
		{
			(*pString) = reinterpret_cast<wchar_t*>(pData);
		}

		delete [] pData;
	}

	if (ERROR_SUCCESS != dwErrToSet)
	{
		::SetLastError(dwErrToSet);
	}

	return(ERROR_SUCCESS == dwErrToSet);
}

bool LeoHelpers::RegQueryExpandStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, bool bExpand, REGSAM sam, std::wstring *pString)
{
	DWORD dwErrToSet = ERROR_SUCCESS;

	DWORD dwType;
	void *pData;

	pString->erase();

	if (! LeoHelpers::RegQueryValue(hKeyParent, szKeyPath, szValueName, sam, &dwType, &pData, NULL) )
	{
		dwErrToSet = ::GetLastError();
	}
	else
	{
		if (REG_SZ == dwType
		||	(REG_EXPAND_SZ == dwType && !bExpand))
		{
			(*pString) = reinterpret_cast<wchar_t*>(pData);
		}
		else if (REG_EXPAND_SZ == dwType && bExpand)
		{
			if (! LeoHelpers::ExpandEnvVars(pString, reinterpret_cast<wchar_t*>(pData)) )
			{
				dwErrToSet = ::GetLastError();
			}
		}
		else
		{
			dwErrToSet = ERROR_INVALID_DATA;
		}

		delete [] pData;
	}

	if (ERROR_SUCCESS != dwErrToSet)
	{
		::SetLastError(dwErrToSet);
	}

	return(ERROR_SUCCESS == dwErrToSet);
}

bool LeoHelpers::RegQueryMultiStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, std::vector< std::wstring > *pVecStrings)
{
	DWORD dwErrToSet = ERROR_SUCCESS;

	DWORD dwType;
	void *pData;

	pVecStrings->clear();

	if (! LeoHelpers::RegQueryValue(hKeyParent, szKeyPath, szValueName, sam, &dwType, &pData, NULL) )
	{
		dwErrToSet = GetLastError();
	}
	else
	{
		if (REG_MULTI_SZ != dwType)
		{
			dwErrToSet = ERROR_INVALID_DATA;
		}
		else
		{
			const wchar_t *msz = reinterpret_cast<wchar_t*>(pData);

			for ( std::wstring strLine = msz; !strLine.empty(); strLine = ( msz = (msz + strLine.length() + 1) ) )
			{
				pVecStrings->push_back( strLine );
			}
		}

		delete [] pData;
	}

	if (ERROR_SUCCESS != dwErrToSet)
	{
		::SetLastError(dwErrToSet);
	}

	return(ERROR_SUCCESS == dwErrToSet);
}

bool LeoHelpers::RegEnumKey(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szPrefixFilter, REGSAM sam, std::vector< std::wstring > *pVecStrings)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	pVecStrings->clear();

	HKEY hKeyToEnum;

	// szKeyPath may be NULL in which case another handle like hKeyParent is openned (and must be closed).
	regRes = ::RegOpenKeyEx(hKeyParent, szKeyPath, 0, sam | KEY_ENUMERATE_SUB_KEYS, &hKeyToEnum);

	if (ERROR_SUCCESS == regRes)
	{
		FILETIME ftLastWriteTime;	// Don't care but documentation doesn't say it can be null.

		// Max key-name   length is 255   chars according to MSDN. (Not sure if that includes the null.) Doesn't hurt to work with more.
		// Max value-name length is 16383 bytes according to MSDN. (Not sure if that includes the null.) Doesn't hurt to work with more.
		wchar_t szNameBuffer[1024];
		const DWORD dwNameBufferSize = (sizeof(szNameBuffer)/sizeof(szNameBuffer[0]));
		DWORD dwcName;

		size_t cPrefixFilter = (NULL != szPrefixFilter) ? wcslen(szPrefixFilter) : 0;

		for(DWORD dwIndex = 0; true; dwIndex++)
		{
			dwcName = dwNameBufferSize;
			regRes = ::RegEnumKeyEx(hKeyToEnum, dwIndex, szNameBuffer, &dwcName, NULL, NULL, NULL, &ftLastWriteTime);

			if (ERROR_SUCCESS == regRes && dwcName >= dwNameBufferSize)
			{
				regRes = ERROR_INSUFFICIENT_BUFFER;
				break;
			}
			else if (ERROR_NO_MORE_ITEMS == regRes)
			{
				regRes = ERROR_SUCCESS;
				break;
			}
			else if (ERROR_SUCCESS != regRes)
			{
				break;
			}
			else
			{
				if (0 == cPrefixFilter || 0 == _wcsnicmp(szNameBuffer, szPrefixFilter, cPrefixFilter))
				{
					pVecStrings->push_back( szNameBuffer + cPrefixFilter );
				}
			}
		}

		LONG closeRes = ::RegCloseKey(hKeyToEnum);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::RegEnumValue(HKEY hKeyParent, const wchar_t *szKeyPath, REGSAM sam, std::vector< std::wstring > *pVecStrings)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	pVecStrings->clear();

	HKEY hKeyToEnum;

	// szKeyPath may be NULL in which case another handle like hKeyParent is openned (and must be closed).
	regRes = ::RegOpenKeyEx(hKeyParent, szKeyPath, 0, sam | KEY_QUERY_VALUE, &hKeyToEnum);

	if (ERROR_SUCCESS == regRes)
	{
		// Max key-name   length is 255   chars according to MSDN. (Not sure if that includes the null.) Doesn't hurt to work with more.
		// Max value-name length is 16383 chars according to MSDN. (Not sure if that includes the null.) Doesn't hurt to work with more.
		// ANSI version of RegEnumValue will go haywire if we give a size larger than 32767 chars/bytes.
		const DWORD dwNameBufferSize = 32767;
		wchar_t *szNameBuffer = new wchar_t[dwNameBufferSize];
		DWORD dwcName;

		for(DWORD dwIndex = 0; true; dwIndex++)
		{
			dwcName = dwNameBufferSize;
			regRes = ::RegEnumValue(hKeyToEnum, dwIndex, szNameBuffer, &dwcName, NULL, NULL, NULL, NULL);

			if (ERROR_SUCCESS == regRes && dwcName >= dwNameBufferSize)
			{
				regRes = ERROR_INSUFFICIENT_BUFFER;
				break;
			}
			else if (ERROR_NO_MORE_ITEMS == regRes)
			{
				regRes = ERROR_SUCCESS;
				break;
			}
			else if (ERROR_SUCCESS != regRes)
			{
				break;
			}
			else
			{
				pVecStrings->push_back( szNameBuffer );
			}
		}

		delete [] szNameBuffer;
		szNameBuffer = NULL;

		LONG closeRes = ::RegCloseKey(hKeyToEnum);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

// Returns false on failure (including if the key does not exist). Use GetLastError().
// The sam argument allow you to pass KEY_WOW64_32KEY or KEY_WOW64_64KEY; pass zero otherwise.
bool LeoHelpers::RegDoesKeyExist(HKEY hKeyParent, const wchar_t *szKeyPath, REGSAM sam)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	HKEY hKeyToClose;

	// szKeyPath may be NULL in which case another handle like hKeyParent is openned (and must be closed).
	regRes = ::RegOpenKeyEx(hKeyParent, szKeyPath, 0, sam | KEY_QUERY_VALUE, &hKeyToClose);

	if (ERROR_SUCCESS == regRes)
	{
		// This looks weird since there's no call before the close. Left it this way to
		// avoid copy & paste errors if this function is copied for another purpose.

		LONG closeRes = ::RegCloseKey(hKeyToClose);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::ExpandEnvVars(std::wstring *pstrOut, const wchar_t *szInput)
{
	size_t bufferSize = (wcslen(szInput) * 3) / 2;

	if (bufferSize > ULONG_MAX)
	{
		bufferSize = ULONG_MAX;
	}

	if (bufferSize < 64)
	{
		bufferSize = 64;
	}

	wchar_t *szResult = new(std::nothrow) wchar_t[bufferSize];

	while(true)
	{
		if (NULL == szResult)
		{
			::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
			return false;
		}

		DWORD dwRes = ::ExpandEnvironmentStrings(szInput, szResult, static_cast< DWORD >( bufferSize ));

		if (dwRes == 0)
		{
			pstrOut->clear();
			delete[] szResult;
			return false;
		}

		if (dwRes <= bufferSize)
		{
			*pstrOut = szResult;
			delete[] szResult;
			return true;
		}

		// Our buffer wasn't big enough. Allocate the size we've been told and try again.
		// Since we're in a loop we'll cope with the env-vars changing between calls, though that is quite unlikely. :)
		bufferSize = dwRes;
		delete[] szResult;
		szResult = new(std::nothrow) wchar_t[bufferSize];
	}
}

// Returns false on failure. Use GetLastError().
// If the string isn't in the @blah.dll,-1234 format then the function returns true and does not modify the string.
// Otherwise, the function tries to look up the string in the given DLL and returns boolean success.
// If the string contains env-vars like %SystemRoot% then you should first expand them via ExpandEnvVars.
bool LeoHelpers::ConvertRegistryStyleLocalisedString(std::wstring &str)
{
	if (str.empty() || str.at(0) != L'@')
	{
		return true; // No conversion needed as it isn't a pointer to a localised string.
	}

	const std::wstring::size_type inLen = str.length();

	std::wstring::size_type pos = str.rfind(L',');

	if (pos <= 1 || pos == std::wstring::npos || pos >= (inLen-1))
	{
		return true; // No conversion needed as it isn't a pointer to a localised string.
	}

	std::wstring strDllPath = str.substr(1, pos - 1);

	// Skip the - at the start of the resource ID. Not sure why it is there, TBH. Need to use a positive ID for the actual look-up.
	if (str.at(pos + 1) == L'-')
	{
		++pos;
	}

	if (pos == std::wstring::npos || pos >= (inLen-1))
	{
		return true; // No conversion needed as it isn't a pointer to a localised string.
	}

	std::wstring strNumber = str.substr(pos + 1);

	if (strNumber.empty())
	{
		return true; // No conversion needed as it isn't a pointer to a localised string.
	}

	for (std::wstring::const_iterator numIter = strNumber.begin(); numIter != strNumber.end(); ++numIter)
	{
		wchar_t c = *numIter;
		switch(c)
		{
		default:
			return true; // No conversion needed as it isn't a pointer to a localised string.
		case L'0':
		case L'1':
		case L'2':
		case L'3':
		case L'4':
		case L'5':
		case L'6':
		case L'7':
		case L'8':
		case L'9':
			break;
		}
	}

	const int iNumber = _wtoi(strNumber.c_str());

	HMODULE hModule = ::LoadLibraryEx(strDllPath.c_str(), NULL, LOAD_LIBRARY_AS_DATAFILE);

	if (hModule == NULL)
	{
		// GetLastError should be set by LoadLibraryEx.
		return false;
	}

	wchar_t *szTemp = NULL;
	int nBufferSize = 128;

	while(true)
	{
		szTemp = new(std::nothrow) wchar_t[nBufferSize + 1]; // +1 because I don't trust LoadString on the edge case.

		if (szTemp == NULL)
		{
			::FreeLibrary(hModule);
			::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
			return false;
		}

		int nLoadRes = ::LoadString(hModule, iNumber, szTemp, nBufferSize);

		if (nLoadRes <= 0)
		{
			// The string wasn't found. Failure.

			DWORD dwErr = ::GetLastError();
			delete[] szTemp;
			::FreeLibrary(hModule);
			::SetLastError(dwErr);
			return false;
		}

		if (nLoadRes < (nBufferSize-1)) // nBufferSize-1 since LoadString is supposed to exclude the null-term from the result count.
		{
			str = szTemp;
			delete[] szTemp;

			if (!::FreeLibrary(hModule))
			{
				// GetLastError should be set by FreeLibrary.
				return false;
			}

			return true;
		}

		// The buffer was almost certainly too small. Increase the size and try again.

		delete[] szTemp;
		szTemp = NULL;
		if (nBufferSize >= (INT_MAX/2)) // The test is >= rather than > because we add 1 to it. Though a 2gig string would be pretty ridiculous anyway. :)
		{
			::FreeLibrary(hModule);
			::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
			return false;
		}
		nBufferSize *= 2;
	}
}
