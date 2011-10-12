#include "stdafx.h"
#include "LeoHelpers_String.h"
#include "LeoHelpers_Reg_Read.h"
#include "LeoHelpers_Reg_Write.h"

bool LeoHelpers::RegSetValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, DWORD dwType, const void *lpData, DWORD cbData)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	bool bCloseKey = false;
	HKEY hKeyToCreateValueIn;

	if ((NULL == szKeyPath) || (0 == _wcsicmp(szKeyPath, L"")))
	{
		// Cannot give NULL as second argument to RegCreateKeyEx() as you can to RegOpenKeyEx()
		hKeyToCreateValueIn = hKeyParent;
	}
	else
	{
		DWORD disposition;

		// We don't need to create the key one level at a time, RegCreateKeyEx() does that.
		regRes = ::RegCreateKeyEx(hKeyParent, szKeyPath, 0, L"",
									REG_OPTION_NON_VOLATILE, sam | KEY_SET_VALUE, NULL,
									&hKeyToCreateValueIn, &disposition);

		if (ERROR_SUCCESS == regRes)
		{
			bCloseKey = true;
		}
	}

	if ((ERROR_SUCCESS == regRes) && (NULL != lpData))
	{
		regRes = ::RegSetValueEx(hKeyToCreateValueIn, szValueName, 0, dwType, reinterpret_cast< const BYTE * >(lpData), cbData);
	}

	if (bCloseKey)
	{
		LONG closeRes = ::RegCloseKey(hKeyToCreateValueIn);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }

		bCloseKey = false;
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::RegSetDWORDValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, DWORD dwData)
{
	return(LeoHelpers::RegSetValue(hKeyParent, szKeyPath, szValueName, sam, REG_DWORD, &dwData, sizeof(DWORD)));
}

bool LeoHelpers::RegSetIntValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, int iData)
{
	if (sizeof(int)==sizeof(DWORD))
	{
		DWORD dwData = *reinterpret_cast< DWORD * >(&iData);

		return(LeoHelpers::RegSetValue(hKeyParent, szKeyPath, szValueName, sam, REG_DWORD, &dwData, sizeof(DWORD)));
	}

	return false;
}

bool LeoHelpers::RegSetBoolValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, bool bData)
{
	DWORD dwData = bData ? 1 : 0;

	return(LeoHelpers::RegSetValue(hKeyParent, szKeyPath, szValueName, sam, REG_DWORD, &dwData, sizeof(DWORD)));
}

bool LeoHelpers::RegSetStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, const wchar_t *szData)
{
	return(LeoHelpers::RegSetValue(hKeyParent, szKeyPath, szValueName, sam, REG_SZ, szData, static_cast< DWORD >((wcslen(szData)+1)*sizeof(szData[0]))));
}

bool LeoHelpers::RegSetMultiStringValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam, const std::vector< std::wstring > *pVecStrings)
{
	bool bResult = false;

	size_t dwBufferSizeChars = 0;
	wchar_t *mszBuffer = LeoHelpers::MultiStringFromVector(pVecStrings, &dwBufferSizeChars);

	if (NULL != mszBuffer)
	{
		bResult = LeoHelpers::RegSetValue(hKeyParent, szKeyPath, szValueName, sam, REG_MULTI_SZ, mszBuffer, static_cast< DWORD >(dwBufferSizeChars * sizeof(wchar_t)));

		delete [] mszBuffer;
	}

	return(bResult);
}

bool LeoHelpers::RegCreateKey(HKEY hKeyParent, const wchar_t *szKeyPath, REGSAM sam)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	HKEY hKeyResult = NULL;

	regRes = ::RegCreateKeyEx(hKeyParent, szKeyPath, 0, NULL, REG_OPTION_NON_VOLATILE, sam, NULL, &hKeyResult, NULL);

	if (ERROR_SUCCESS == regRes)
	{
		LONG closeRes = ::RegCloseKey(hKeyResult);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::RegDeleteKey(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szKeyToDelete, REGSAM sam, bool bDeleteSubKeys)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	std::wstring strBufferParent;
	std::wstring strBufferKey;

	// MSDN is ambiguous about whether ::RegDeleteKey requires the named key to be directly below the parent key or just somewhere below it.
	// Assume it needs to be directly below it, and move bits of the path arguments around to ensure that is the case.
	if (szKeyToDelete != NULL)
	{
		size_t pos = wcslen(szKeyToDelete);
		if (pos == 0 || szKeyToDelete[0] == L'\\' || szKeyToDelete[pos-1] == L'\\')
		{
			assert(false);
			::SetLastError(ERROR_INVALID_PARAMETER);
			return false;
		}
		while(pos > 0)
		{
			if (szKeyToDelete[pos-1] != L'\\')
			{
				--pos;
				continue;
			}

			strBufferParent = (szKeyPath != NULL) ? szKeyPath : L"";
			if (!strBufferParent.empty() && strBufferParent.at(strBufferParent.length() - 1) != L'\\')
			{
				strBufferParent += L'\\';
			}
			strBufferParent.append(szKeyToDelete, pos - 1);

			strBufferKey = szKeyToDelete + pos;

			szKeyPath = strBufferParent.c_str();
			szKeyToDelete = strBufferKey.c_str();

			break;
		}
	}

	// szKeyPath may be empty/NULL but szKeyToDelete must not be.
	if (szKeyToDelete == NULL || szKeyToDelete[0] == L'\0')
	{
		assert(false);
		::SetLastError(ERROR_INVALID_PARAMETER);
		return false;
	}

	if (bDeleteSubKeys)
	{
		std::wstring strDeletePath;

		// szKeyPath may be null if something directly below hKeyParent is being deleted.
		if (szKeyPath != NULL)
		{
			strDeletePath = szKeyPath;
		}

		if (!strDeletePath.empty() && strDeletePath.at(strDeletePath.length() - 1) != L'\\')
		{
			strDeletePath += L'\\';
		}

		strDeletePath += szKeyToDelete;

		std::vector< std::wstring > vecNames;

		if (!LeoHelpers::RegEnumKey(hKeyParent, strDeletePath.c_str(), NULL, sam, &vecNames))
		{
			if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
		}
		else
		{
			for (std::vector< std::wstring >::const_iterator nameIter = vecNames.begin(); nameIter != vecNames.end(); ++nameIter)
			{
				if (nameIter->empty())
				{
					assert(false);
					continue;
				}

				// Recursive call.
				if (!LeoHelpers::RegDeleteKey(hKeyParent, strDeletePath.c_str(), nameIter->c_str(), sam, true))
				{
					if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
				}
			}
		}
	}

	LONG regResRecursion = regRes;

	HKEY hKeyToDeleteUnder;

	// szKeyPath may be NULL in which case another handle like hKeyParent is openned (and must be closed).
	regRes = ::RegOpenKeyEx(hKeyParent, szKeyPath, 0, sam | DELETE, &hKeyToDeleteUnder);

	if (ERROR_SUCCESS == regRes)
	{
		// Note: It does not seem neccessary to use RegDeleteKeyEx (added in XP-x64 and Vista) to delete keys
		//       in a particular SAM, provided we specify the SAM when calling RegOpenKeyEx like we do above.
		//       I suspect RegDeleteKeyEx is only needed if you want to delete a key path relative to one of the root
		//       keys such as HKEY_LOCAL_MACHINE, but even then I expect you could open a new handle to the root
		//       with the SAM specified and it should work fine.
		regRes = ::RegDeleteKey(hKeyToDeleteUnder, szKeyToDelete);

		if (ERROR_SUCCESS != regRes && ERROR_SUCCESS != regResRecursion)
		{
			// Use the recursive error if the main delete failed as the latter is almost certainly a result of the former.
			// OTOH, if the main delete succeeded somehow even after the recursion failed then that's still a success.
			regRes = regResRecursion;
		}

		LONG closeRes = ::RegCloseKey(hKeyToDeleteUnder);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::RegDeleteValue(HKEY hKeyParent, const wchar_t *szKeyPath, const wchar_t *szValueName, REGSAM sam)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	HKEY hKeyToDeleteUnder;

	// szKeyPath may be NULL in which case another handle like hKeyParent is openned (and must be closed).
	regRes = ::RegOpenKeyEx(hKeyParent, szKeyPath, 0, sam | KEY_SET_VALUE, &hKeyToDeleteUnder);

	if (ERROR_SUCCESS == regRes)
	{
		regRes = ::RegDeleteValue(hKeyToDeleteUnder, szValueName);

		LONG closeRes = ::RegCloseKey(hKeyToDeleteUnder);
		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}

bool LeoHelpers::RegCopyKey(HKEY hKeySourceParent, REGSAM samParent, HKEY hKeyDestParent, REGSAM samDest, const wchar_t *szKeyPath)
{
	// Documentation doesn't state that registry functions call SetLastError().
	// We need to preserve the error values they return through clean-up anyway.
	LONG regRes = ERROR_SUCCESS;

	if (szKeyPath == NULL || szKeyPath[0] == L'\0')
	{
		assert(false);
		::SetLastError(ERROR_INVALID_PARAMETER);
		return false;
	}

	HKEY hKeyDestActual = NULL;

	regRes = ::RegCreateKeyEx(hKeyDestParent, szKeyPath, 0, NULL, REG_OPTION_NON_VOLATILE, samDest | KEY_WRITE, NULL, &hKeyDestActual, NULL);

	if (ERROR_SUCCESS == regRes)
	{
		std::vector< std::wstring > vecNames;

		// Values first.

		if (!LeoHelpers::RegEnumValue(hKeySourceParent, szKeyPath, samParent, &vecNames))
		{
			if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
		}
		else
		{
			for (std::vector< std::wstring >::const_iterator nameIter = vecNames.begin(); nameIter != vecNames.end(); ++nameIter)
			{
				DWORD dwType = REG_NONE;
				void *pData = NULL;
				DWORD cbData = 0;

				if (!LeoHelpers::RegQueryValue(hKeySourceParent, szKeyPath, nameIter->c_str(), samParent, &dwType, &pData, &cbData))
				{
					if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
				}
				else
				{
					if (!LeoHelpers::RegSetValue(hKeyDestActual, NULL, nameIter->c_str(), samDest, dwType, pData, cbData))
					{
						if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
					}

					delete[] pData;
				}
			}
		}

		// We can close the destination key now.
		LONG closeRes = ::RegCloseKey(hKeyDestActual);

		// Keys second.

		if (!LeoHelpers::RegEnumKey(hKeySourceParent, szKeyPath, NULL, samParent, &vecNames))
		{
			if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
		}
		else
		{
			std::wstring strChildPath;

			for (std::vector< std::wstring >::const_iterator nameIter = vecNames.begin(); nameIter != vecNames.end(); ++nameIter)
			{
				if (nameIter->empty())
				{
					assert(false);
					continue;
				}

				strChildPath = szKeyPath;

				assert(!strChildPath.empty()); // enforced at the start of the function.

				if (!strChildPath.empty() && strChildPath.at(strChildPath.length() - 1) != L'\\')
				{
					strChildPath += L'\\';
				}

				strChildPath += *nameIter;

				// Recursive call.
				if (!LeoHelpers::RegCopyKey(hKeySourceParent, samParent, hKeyDestParent, samDest, strChildPath.c_str()))
				{
					if (ERROR_SUCCESS == regRes) { regRes = ::GetLastError(); }
				}
			}
		}

		if (ERROR_SUCCESS == regRes) { regRes = closeRes; }
	}

	if (ERROR_SUCCESS != regRes)
	{
		::SetLastError(regRes);
	}

	return(ERROR_SUCCESS == regRes);
}
