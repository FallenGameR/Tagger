#include "stdafx.h"
#include "LeoHelpers_String.h"

// We always use the C locale. It's assumed that any case-insensitive string
// comparison, or upper/lower-casing of strings, is done to compare either
// file paths (or extensions), or internal data (e.g. CLSID strings), rather
// than anything which should be locale-dependent. We want consistency across
// machines and executions, not with the user's locale.
const _locale_t LeoHelpers::LocaleC = _create_locale(LC_ALL, "");

std::wstring LeoHelpers::StringTrim(const std::wstring &strIn, const wchar_t *szTrimChars, bool bTrimLeft, bool bTrimRight)
{
	std::wstring::size_type leftOffset  = 0;
	std::wstring::size_type rightOffset = strIn.length();

	if (bTrimLeft)
	{
		leftOffset = strIn.find_first_not_of(szTrimChars);
	}

	if (bTrimRight)
	{
		rightOffset = strIn.find_last_not_of(szTrimChars);

		if (rightOffset != std::wstring::npos)
		{
			++rightOffset;
		}
	}

	if (leftOffset == std::wstring::npos
	||	rightOffset == std::wstring::npos
	||	leftOffset >= rightOffset)
	{
		return L"";
	}

	return strIn.substr(leftOffset, rightOffset - leftOffset);
}

// delete[] the result.
// Returns NULL on failure.
wchar_t *LeoHelpers::VStringAllocAndFormat(const wchar_t *format, va_list args)
{
	// Leo 16/Jan/2009: Instead of defaulting to 64 bytes for the initial buffer size, make it 1.5* the format string length.
	size_t bufferSize = (wcslen(format) * 3) / 2;

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
			break;
		}

		int formatRes = _vsnwprintf_s(szResult, bufferSize, _TRUNCATE, format, args);

		// formatRes negative on error; (bufferSize == formatRes) should be redundant for _vsntprintf.

		if ((0 > formatRes) || (bufferSize == formatRes))
		{
			delete [] szResult;
			bufferSize *= 2;
			szResult = new(std::nothrow) wchar_t[bufferSize];
		}
		else
		{
			break;
		}
	}

	return(szResult);
}

// Returns boolean success.
bool LeoHelpers::StringFormatToString(std::wstring *pResult, const wchar_t *format, ...)
{
	va_list args;
	va_start(args, format);
	wchar_t *result = VStringAllocAndFormat(format, args);
	va_end(args);

	if (result == NULL)
	{
		pResult->clear();
		return false;
	}

	*pResult = result;
	delete[] result;

	return true;
}

// Returns boolean success.
bool LeoHelpers::VStringFormatToString(std::wstring *pResult, const wchar_t *format, va_list args)
{
	wchar_t *result = VStringAllocAndFormat(format, args);

	if (result == NULL)
	{
		pResult->clear();
		return false;
	}

	*pResult = result;
	delete[] result;

	return true;
}

wchar_t *LeoHelpers::MultiStringFromVector(const std::vector< std::wstring > *pVecStrings, size_t *pLengthChars)
{
	if (NULL != pLengthChars)
	{
		*pLengthChars = 0;
	}

	// If the vector is empty then, technically, only a single null should be returned. However, some things are buggy
	// and look for a double-null terminator without considering what happens in the empty-list case. Because of them,
	// an empty list will generate a string with two nulls, but the *pLengthChars will still be set to 1. So you can
	// safely pass the result to buggy code but if you copy the buffer and want the copy to be safe for buggy code as well
	// then you might want to add an extra null in the empty case.

	size_t bufferSize = 1; // Final null character.

	if (pVecStrings->empty())
	{
		++bufferSize; // Extra NULL for buggy code that doesn't handle empty lists properly.
	}
	else
	{
		for (std::vector< std::wstring >::const_iterator pStr = pVecStrings->begin(); pStr != pVecStrings->end(); ++pStr)
		{
			if (!pStr->empty())
			{
				bufferSize += (pStr->length() + 1);
			}
		}
	}

	wchar_t *mszBuffer = new(std::nothrow) wchar_t[ bufferSize ];

	if (NULL == mszBuffer)
	{
		::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
		return NULL;
	}

	if (pVecStrings->empty())
	{
		mszBuffer[0] = L'\0'; // "Final" null character.
		mszBuffer[1] = L'\0'; // Extra NULL for buggy code that doesn't handle empty lists properly.
		--bufferSize; // Don't reflect the extra null we allocated in the "official" buffer size.
	}
	else
	{
		size_t bufferLeft = bufferSize;
		size_t partLen;

		wchar_t *szCurrent = mszBuffer;

		for (std::vector< std::wstring >::const_iterator pStr = pVecStrings->begin(); pStr != pVecStrings->end(); ++pStr)
		{
			if (!pStr->empty())
			{
				if (0 == wcscpy_s(szCurrent, bufferLeft, pStr->c_str()))
				{
					partLen = (pStr->length() + 1);
					szCurrent += partLen;
					bufferLeft -= partLen;
				}
			}
		}

		szCurrent[0] = L'\0'; // Final null character.
	}

	if (NULL != pLengthChars)
	{
		*pLengthChars = bufferSize;
	}

	return mszBuffer;
}

// pp -- Pointer to buffer to tokenize. Will be advanced to the next position string. Set to NULL after the last token is extracted.
// delimiters -- Characters which delimit the string.
// terminators -- Characters which end the string. Can be NULL.
// bSkipEmpty -- Set true to skip any empty strings. (e.g. someone typed "blah,,blah")
// bQuotes -- Set to true to handle double-quotes (") around items. A quoted empty string is NOT skipped even if bSkipEmpty is set. Terminators in quotes are ignored and included in the tokenized strings.
// bCSVQuotes -- (Ignored if bQuotes false) -- Inside quoted strings, two double-quotes next to each other will be output as one double-quote and not signify the end of the string. A string like "Blah""blah" will become blah"blah.
// buf -- Buffer to extract in to. Set to NULL not to extract.
// bufSize -- Size of buf if it is non-NULL. Will truncate string if buf too small.
// Leading and trailing spaces are always stripped.
// Returns false iff nothing could be extracted (i.e. *pp was NULL)
// pp and the contents of buf are only valid after calling if true is returns.
// Lame function should allocate us a suitably sized buffer for each string... :-)
// But hey, this isn't a public OS function and you can just give a buffer the size of the input...
bool LeoHelpers::Tokenize(const wchar_t **pp, const wchar_t *delimiters, const wchar_t *terminators, wchar_t *buf, size_t bufSize, bool bSkipEmpty, bool bQuotes, bool bCSVQuotes)
{
	bool bResult = false;

	if (NULL == terminators)
	{
		terminators = L"";
	}

	if (NULL != *pp)
	{
		// Remove leading spaces.
		// Also, skip leading delimiters if we're to skip empties.
		// (Important that we skip the spaces here so that they're not included if it's the first string.)
		// (Note we must explicitly check for the terminator before checking the delimiters else we ask if
		// the terminator is in the delimiters string.)
		while ( iswspace(**pp) || ((L'\0' != **pp) && (NULL == wcschr(terminators, **pp)) && bSkipEmpty && (NULL != wcschr(delimiters, **pp))) )
		{
			(*pp)++;
		}

		if (bSkipEmpty && ( (L'\0' == **pp) || (NULL != wcschr(terminators, **pp)) ))
		{
			// Last item is empty and we're skipping empties.
			*pp = NULL;
		}
		else
		{
			wchar_t *bufOrig = buf;

			bResult = true; // We got somethin'!

			bool bFindQuote = false;

			if (bQuotes && (L'"' == **pp))
			{
				(*pp)++; // Skip over the starting quote.
				bFindQuote = true; // Copy string until the ending quote rather than a delimiter.
			}

			// There is some string left, extract what's next.
			// Ignore terminators and delmiters if we're looking for the end-quote.
			while (L'\0' != **pp)
			{
				if (bFindQuote)
				{
					if (L'"' == (*pp)[0])
					{
						if (bCSVQuotes && (L'"' == (*pp)[1]))
						{
							(*pp)++; // It is a pair of quotes, skip one and copy the other.
						}
						else
						{
							break; // It is a closing quote.
						}
					}
				}
				else if ((NULL != wcschr(delimiters,  **pp))
				||		 (NULL != wcschr(terminators, **pp)))
				{
					// Not looking for quotes and found a delimiter or a terminator
					break;
				}

				if ((NULL != buf) && (bufSize > 1)) // Keep 1 character for the terminator.
				{
					*buf = **pp;
					bufSize--;
					buf++;
				}

				(*pp)++; // Must increment *pp here as copying to buf is optional and may also be trunctated.
			}

			// Handles the case where a zero-length buffer is passed in.
			if ((NULL != buf) && (bufSize > 0))
			{
				*buf = L'\0'; // NULL-terminate it.

				// Remove trailing spaces if not quoted.
				if (! bFindQuote )
				{
					while (buf > bufOrig)
					{
						if (iswspace(*(--buf)))
						{
							*buf = L'\0';
						}
						else
						{
							break;
						}
					}
				}
			}

			// If quoted, move over the closing quote and skip up to the next delimiter.
			if (bFindQuote && (L'\0' != **pp) && (NULL == wcschr(terminators, **pp)))
			{
				do
				{
					(*pp)++;
				} while ((L'\0' != **pp) && (NULL == wcschr(terminators, **pp)) && (NULL == wcschr(delimiters, **pp)));
			}

			// Move over the delimiter (unless we're at the end of the string).
			if ((L'\0' != **pp) && (NULL == wcschr(terminators, **pp)) && (NULL != wcschr(delimiters, **pp)))
			{
				(*pp)++;

				// Skip spaces before the next token. (Useful for when buf==NULL)
				while ((L'\0' != **pp) && (NULL == wcschr(terminators, **pp)) && iswspace(**pp))
				{
					(*pp)++;
				}
			}
			else
			{
				// We're returning the last thing on the line.
				*pp = NULL;
			}
		}
	}

	return(bResult);
}

/* Code to test how the MB/WC APIs behave:
	wchar_t szWide[16];

	char *aszTests[] =
	{
		"",
		"1",
		"12",
		"123",
		"1234",
		"12345",
		"123456",
		"1234567",
		"12345678",
		"123456789",
		"123456789A",
		"123456789AB",
		"123456789ABC"

	};

	for(int m = 0; m < 4; ++m)
	{
		if (m&1)
		{
			if (m&2)
			{
				printf("Length-specified strings (excluding null in length), and request required size:\n");
			}
			else
			{
				printf("Length-specified strings (excluding null in length):\n");
			}
		}
		else
		{
			if (m&2)
			{
				printf("Null terminated strings, and request required size:\n");
			}
			else
			{
				printf("Null terminated strings:\n");
			}
		}

		for(int t = 0; t < _countof(aszTests); ++t)
		{
			for(int i = 0; i < _countof(szWide); ++i)
			{
				szWide[i] = (i < 9 ? L'x' : L'!');
			}
			szWide[_countof(szWide)-1] = L'\0';

			// Change the +0 to a +1 to see how it behaves when the length is specified but does include the null. (Then it's the same as using -1 as the length.)
			int res = ::MultiByteToWideChar(CP_UTF8, 0, aszTests[t], (m&1) ? strlen(aszTests[t]) +  0 : -1, szWide, (m&2) ? 0 : 10);

			int nullPos;

			for (nullPos = 0; nullPos < _countof(szWide); ++nullPos)
			{
				if (szWide[nullPos] == L'\0')
				{
					break;
				}
			}

			if (nullPos == _countof(szWide) - 1)
			{
				printf("%12s - res = %2d, nullPos = NONE - %S\n", aszTests[t], res, szWide);
			}
			else if (nullPos > 9)
			{
				printf("%12s - res = %2d, nullPos = %4d (ILLEGAL) - %S\n", aszTests[t], res, nullPos, szWide);
			}
			else
			{
				printf("%12s - res = %2d, nullPos = %4d - %S\n", aszTests[t], res, nullPos, szWide);
			}
		}

		printf("\n");
	}
*/


// Returns 0 on failure (use GetLastError()), else the number of wide characters written including the null.
// delete [] *pwcStringResult when finished with it.
// numInputBytes -1 for null-terminated string.
int LeoHelpers::MBtoWC(wchar_t **pwcStringResult, const char *mbStringInput, int numInputBytes, DWORD dwCodePage)
{
	*pwcStringResult = NULL;

	if (numInputBytes == 0)
	{
		*pwcStringResult = new(std::nothrow) wchar_t[1];

		if (NULL == *pwcStringResult)
		{
			::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
			return 0;
		}

		(*pwcStringResult)[0] = L'\0';
		return 1;
	}

	int wcBufSize = ::MultiByteToWideChar(dwCodePage, 0, mbStringInput, numInputBytes, NULL, 0);

	if (wcBufSize == 0)
	{
		return 0;
	}

	if (numInputBytes != -1 && mbStringInput[numInputBytes-1] != '\0')
	{
		// If the string length is specified, and it doesn't contain a null, then MultiByteToWideChar will not include space for the null.
		// We'll add and allocate room for that extra null, whether the caller requires it or not. If the caller doesn't then it's harmless.
		++wcBufSize;
	}

	*pwcStringResult = new(std::nothrow) wchar_t[wcBufSize];

	if (NULL == *pwcStringResult)
	{
		::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
		return 0;
	}

	int convRes = ::MultiByteToWideChar(dwCodePage, 0, mbStringInput, numInputBytes, *pwcStringResult, wcBufSize);

	(*pwcStringResult)[wcBufSize-1] = L'\0'; // No matter what, the buffer is terminated.

	if (convRes <= 0 || convRes > wcBufSize)
	{
		assert(convRes==0); // Real WTF from the API if it isn't zero.

		delete[] *pwcStringResult;
		*pwcStringResult = NULL;

		return 0;
	}

	if (numInputBytes != -1 && mbStringInput[numInputBytes-1] != '\0' && convRes < wcBufSize)
	{
		++convRes; // Bump for the null that we wrote, which was not written by the API or included in its count.
	}

	// Finally, remove any left-surrogates from the end of the string.

	while(convRes > 1 && LeoHelpers::IsUtf16LeftSurrogate((*pwcStringResult)[convRes-2])) // -1 is the current null; -2 is the last character.
	{
		(*pwcStringResult)[convRes-2] = L'\0';
		--convRes;
	}

	// Ideally we'd ensure three are no partial multi-byte chars at the end of the string but I can't see a good way to do this
	// for the same dwCodePage argument which WideCharToMultiByte takes. CharPrevExA/CharNextExA are almost what we want, but not quite.

	return convRes;
}

// Returns 0 on failure (use GetLastError()), else the number of characters written.
// delete [] *pmbStringResult when finished with it.
// numInputWords -1 for null-terminated string.
int LeoHelpers::WCtoMB(char **pmbStringResult, const wchar_t *wcStringInput, int numInputWords, DWORD dwCodePage)
{
	*pmbStringResult = NULL;

	if (numInputWords == 0)
	{
		*pmbStringResult = new(std::nothrow) char[1];

		if (NULL == *pmbStringResult)
		{
			::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
			return 0;
		}

		(*pmbStringResult)[0] = '\0';
		return 1;
	}

	int mbBufSize = ::WideCharToMultiByte(dwCodePage, 0, wcStringInput, numInputWords, NULL, 0, NULL, NULL);

	if (mbBufSize == 0)
	{
		return 0;
	}

	if (numInputWords != -1 && wcStringInput[numInputWords-1] != L'\0')
	{
		// If the string length is specified, and it doesn't contain a null, then WideCharToMultiByte will not include space for the null.
		// We'll add and allocate room for that extra null, whether the caller requires it or not. If the caller doesn't then it's harmless.
		++mbBufSize;
	}

	*pmbStringResult = new(std::nothrow) char[mbBufSize];

	if (NULL == *pmbStringResult)
	{
		::SetLastError(ERROR_NOT_ENOUGH_MEMORY);
		return 0;
	}

	int convRes = ::WideCharToMultiByte(dwCodePage, 0, wcStringInput, numInputWords, *pmbStringResult, mbBufSize, NULL, NULL);

	(*pmbStringResult)[mbBufSize-1] = '\0'; // No matter what, the buffer is terminated.

	if (convRes <= 0 || convRes > mbBufSize)
	{
		assert(convRes==0); // Real WTF from the API if it isn't zero.

		delete[] (*pmbStringResult);
		*pmbStringResult = NULL;

		return 0;
	}

	if (numInputWords != -1 && wcStringInput[numInputWords-1] != L'\0' && convRes < mbBufSize)
	{
		++convRes; // Bump for the null tht we wrote, which was not written by the API or included in its count.
	}

	return(convRes);
}

// Returns the new null position.
size_t LeoHelpers::TruncateString(wchar_t *szBuffer, size_t idxNull, size_t cchSpaceAfterNull, bool bTruncateWithElipsis)
{
	size_t ec = 0; // This is how many spaces for dots we want (but may not get if the buffer is too small).

	if (bTruncateWithElipsis && cchSpaceAfterNull < 3)
	{
		ec = 3 - cchSpaceAfterNull; // How much of the string we need to eat in order to fit three dots before the null.
	}

	cchSpaceAfterNull = 0; // nothing else should use it.

	while(idxNull > 0 && (ec > 0 || LeoHelpers::IsUtf16LeftSurrogate(szBuffer[idxNull-1])))
	{
		if (ec > 0)
		{
			--ec;
		}

		szBuffer[--idxNull] = L'\0';
	}

	assert(szBuffer[idxNull] == L'\0');

	if (bTruncateWithElipsis)
	{
		// Add up to three '.' in whatever space we've made.
		while(ec < 3)
		{
			szBuffer[  idxNull] = L'.';
			szBuffer[++idxNull] = L'\0';
			++ec;
		}
	}

	assert(szBuffer[idxNull] == L'\0');

	return idxNull;
}

bool LeoHelpers::MBtoWCInPlaceTruncate(const char *szSource, int cbInputBytes, wchar_t *szDestBuffer, int cchBufferSize, DWORD dwCodePage, bool bTruncateWithElipsis, bool *pbWasTruncated)
{
	if (pbWasTruncated != NULL) { *pbWasTruncated = false; }

	assert(cchBufferSize > 0);

	if (cchBufferSize < 1)
	{
		if (pbWasTruncated != NULL) { *pbWasTruncated = true; }
		return false;
	}

	if (cbInputBytes == 0)
	{
		szDestBuffer[0] = L'\0';
		return true;
	}

	int fres = ::MultiByteToWideChar(dwCodePage, 0, szSource, cbInputBytes, szDestBuffer, cchBufferSize);

	szDestBuffer[cchBufferSize-1] = L'\0'; // Whatever happened, it's null terminated.

	if (cbInputBytes != -1 && szSource[cbInputBytes-1] != '\0' && fres > 0 && fres <= cchBufferSize)
	{
		if (fres < cchBufferSize)
		{
			szDestBuffer[fres] = L'\0'; // We need to add a null to the string, and there is room for it.
			++fres; // Account for the fact we added it.
		}
		else
		{
			// The buffer has no null, and no room for the null. Turn this success into a failure so it is truncated.
			fres = 0;
			::SetLastError(ERROR_INSUFFICIENT_BUFFER);
		}
	}

	if (fres > 0 && fres <= cchBufferSize)
	{
		// Remove any left-surrogates from the end of the string.
		while(fres > 1 && LeoHelpers::IsUtf16LeftSurrogate(szDestBuffer[fres-2])) // -1 is the current null; -2 is the last character.
		{
			szDestBuffer[fres-2] = L'\0';
			--fres;
		}

		return true;
	}
	else if (fres == 0)
	{
		if (::GetLastError() != ERROR_INSUFFICIENT_BUFFER)
		{
			// Failed for some other reason. Who knows what's in the buffer. Nuke it, return failure, thank-you-drive-through.
			szDestBuffer[0] = L'\0';
			return false;
		}
		else
		{
			if (pbWasTruncated != NULL) { *pbWasTruncated = true; }

			// Assumption: MultiByteToWideChar filled the buffer as much as it could. (This isn't actually documented -- FFS! -- but is what happens.)
			// The buffer will not be null-terminated yet. We also need to remove any partial surrogates and, if requested, add the elpisis (if it'll fit).
			// Making room for the elipsis may create an orphaned surrogate as well.

			fres = cchBufferSize; // This is how many characters were written. We know this is at least 1. No null has been written yet.

			szDestBuffer[--fres] = L'\0'; // The buffer is now null terminated. fres may now be zero.

			LeoHelpers::TruncateString(szDestBuffer, fres, 0, bTruncateWithElipsis);

			return true;
		}
	}
	else
	{
		assert(false); // Getting here would be a real WTF on the API's part.
		szDestBuffer[0] = L'\0';
		return false;
	}
}
