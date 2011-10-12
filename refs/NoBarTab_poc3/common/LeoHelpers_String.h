#pragma once

// See also: LeoHelpers_Misc has StringCache and StringLoader.
//           LeoHelpers_Opus has OpusStringLoader.

namespace LeoHelpers
{
	extern const _locale_t LocaleC;

	inline wchar_t WideCharUpper(wchar_t c) { return _towupper_l(c, LocaleC); }
	inline wchar_t WideCharLower(wchar_t c) { return _towlower_l(c, LocaleC); }

	inline int WideStrCmpUpper(const wchar_t *x, const wchar_t *y)
	{
		wchar_t ux, uy;
		while(*x && *y)
		{
			ux = WideCharUpper(*x++);
			uy = WideCharUpper(*y++);
			if (ux < uy) return -1;
			if (uy < ux) return 1;
		}
		if (*y) return -1;
		if (*x) return 1;
		return 0;
	}

	inline int WideStrCmpUpper(const std::wstring &x, const std::wstring &y) { return WideStrCmpUpper(x.c_str(), y.c_str()); }

	// Avoids the WideCharUpper calls for the second argument if you know it's already upper case.
	inline int WideStrCmpUpperLeftOnly(const wchar_t *x, const wchar_t *yAlreadyUpper)
	{
#ifdef _DEBUG
		for (const wchar_t *ydbg = yAlreadyUpper; *ydbg; ++ydbg)
		{
			assert(*ydbg == WideCharUpper(*ydbg));
		}
#endif

		wchar_t ux, uy;
		while(*x && *yAlreadyUpper)
		{
			ux = WideCharUpper(*x++);
			uy = *yAlreadyUpper++;
			if (ux < uy) return -1;
			if (uy < ux) return 1;
		}
		if (*yAlreadyUpper) return -1;
		if (*x) return 1;
		return 0;
	}

	inline int WideStrCmpUpperLen(const wchar_t *x, const wchar_t *y, size_t n)
	{
		wchar_t ux, uy;
		while(*x && *y && n)
		{
			ux = WideCharUpper(*x++);
			uy = WideCharUpper(*y++);
			if (ux < uy) return -1;
			if (uy < ux) return 1;
			--n;
		}
		if (n == 0) return 0;
		if (*y) return -1;
		if (*x) return 1;
		return 0;
	}

	inline int WideStrCmpUpperLen(const std::wstring &x, const std::wstring &y, size_t n) { return WideStrCmpUpperLen(x.c_str(), y.c_str(), n); }

	inline char CharToUpper(unsigned char c)
	{
		if (__isascii(c) && _islower_l(c, LocaleC))
		{
			return _toupper_l(c, LocaleC);
		}
		return c;
	}

	inline char CharToLower(unsigned char c)
	{
		if (__isascii(c) && _isupper_l(c, LocaleC))
		{
			return _tolower_l(c, LocaleC);
		}
		return c;
	}

	inline int StrCmpUpperLenLeftOnly(const char *x, const char *yAlreadyUpper, size_t n)
	{
		char ux, uy;
		while(*x && *yAlreadyUpper && n)
		{
			ux = CharToUpper(*x++);
			uy = *yAlreadyUpper++;
			if (ux < uy) return -1;
			if (uy < ux) return 1;
			--n;
		}
		if (n == 0) return 0;
		if (*yAlreadyUpper) return -1;
		if (*x) return 1;
		return 0;
	}

	inline int __cdecl WideStrBSearchCmp(void *pvContext, const void *pvLeft, const void *pvRight)
	{
		return ::wcscmp(*reinterpret_cast<const wchar_t * const *>(pvLeft), *reinterpret_cast<const wchar_t * const *>(pvRight));
	}

	inline int __cdecl WideStrBSearchCmpUpper(void *pvContext, const void *pvLeft, const void *pvRight)
	{
		return LeoHelpers::WideStrCmpUpper(reinterpret_cast<const wchar_t *>(pvLeft), reinterpret_cast<const wchar_t *>(pvRight));
	}

	struct LessWCharCaseSensitive
	{
		bool operator()(const wchar_t *x, const wchar_t *y) const
		{ return 0 > wcscmp(x, y); }
	};

//	struct LessCaseSensitive : public std::binary_function<std::wstring, std::wstring, bool>
//	{
//		bool operator()(const std::wstring &x, const std::wstring &y) const
//		{ return x < y; }
//	};

	struct LessUpperCase // : public std::binary_function<std::wstring, std::wstring, bool>
	{
		bool operator()(const std::wstring &x, const std::wstring &y) const
		{ return 0 > WideStrCmpUpper(x.c_str(), y.c_str()); }
	};

	class LessStringEither
	{
	private:
		bool m_bCaseSensitive;
	public:
		LessStringEither(bool bCaseSensitive) : m_bCaseSensitive(bCaseSensitive) { }

		bool IsCaseSensitive() const { return m_bCaseSensitive; }

		bool operator()(const std::wstring &x, const std::wstring &y) const
		{
			if (!m_bCaseSensitive)
			{
				return WideStrCmpUpper(x.c_str(), y.c_str()) < 0;
			}
			else
			{
				return wcscmp(x.c_str(), y.c_str()) < 0;
			}
		}
	};

	struct LessGuidMemory
	{
		bool operator()(const GUID * const x, const GUID * const y) const
		{ return 0 > memcmp(x, y, sizeof(GUID)); }
	};

	inline bool Body_LessVectorUpperCase(const std::vector< std::wstring > &x, const std::vector< std::wstring > &y)
	{
		std::vector< std::wstring >::const_iterator iterX = x.begin();
		std::vector< std::wstring >::const_iterator iterY = y.begin();
		int r;

		while(iterX != x.end() && iterY != y.end())
		{
			r = WideStrCmpUpper(iterX->c_str(), iterY->c_str());
			if (r != 0)
			{
				return (r < 0);
			}
			++iterX;
			++iterY;
		}
		return(iterX == x.end() && iterY != y.end());
	}

	inline bool Body_LessVectorCaseSensitive(const std::vector< std::wstring > &x, const std::vector< std::wstring > &y)
	{
		std::vector< std::wstring >::const_iterator iterX = x.begin();
		std::vector< std::wstring >::const_iterator iterY = y.begin();
		int r;

		while(iterX != x.end() && iterY != y.end())
		{
			r = wcscmp(iterX->c_str(), iterY->c_str());
			if (r != 0)
			{
				return (r < 0);
			}
			++iterX;
			++iterY;
		}
		return(iterX == x.end() && iterY != y.end());
	}

	// This lets you have a set < vector < wstring > > with case-insensitive lookups.
	struct LessVectorUpperCase
	{
		bool operator()(const std::vector< std::wstring > &x, const std::vector< std::wstring > &y) const
		{
			return Body_LessVectorUpperCase(x, y);
		}
	};

	struct LessVectorCaseSensitive
	{
		bool operator()(const std::vector< std::wstring > &x, const std::vector< std::wstring > &y) const
		{
			return Body_LessVectorCaseSensitive(x, y);
		}
	};

	// This lets you have a set < vector < wstring > > with either case-sensitive or case-insensitive lookups, determined at runtime.
	class LessVectorStringEither
	{
	private:
		bool m_bCaseSensitive;
	public:
		LessVectorStringEither(bool bCaseSensitive) : m_bCaseSensitive(bCaseSensitive) { }

		bool IsCaseSensitive() const { return m_bCaseSensitive; }

		bool operator()(const std::vector< std::wstring > &x, const std::vector< std::wstring > &y) const
		{
			if (!m_bCaseSensitive)
			{
				return Body_LessVectorUpperCase(x, y);
			}
			else
			{
				return Body_LessVectorCaseSensitive(x, y);
			}
		}
	};

	inline void ToUpper(std::string *pStrOutput, const char *szInput)
	{
		pStrOutput->clear();
		while(*szInput)
		{
			pStrOutput->push_back(LeoHelpers::CharToUpper(*szInput++));
		}
	}

	inline void ToUpper(std::wstring *pstrOutput, const wchar_t *szInput)
	{
		size_t len = wcslen(szInput);

		pstrOutput->clear();
		pstrOutput->reserve(len);

		std::transform(szInput, szInput + len, std::back_inserter< std::wstring >(*pstrOutput), WideCharUpper);
	}

	inline void ToLower(std::wstring *pstrOutput, const wchar_t *szInput)
	{
		size_t len = wcslen(szInput);

		pstrOutput->clear();
		pstrOutput->reserve(len);

		std::transform(szInput, szInput + len, std::back_inserter< std::wstring >(*pstrOutput), WideCharLower);
	}

	inline void ToUpper(std::wstring *pstrOutput, const std::wstring &strInput)
	{
		pstrOutput->clear();
		pstrOutput->reserve(strInput.length());

		std::transform(strInput.begin(), strInput.end(), std::back_inserter< std::wstring >(*pstrOutput), WideCharUpper);
	}

	inline void ToLower(std::wstring *pstrOutput, const std::wstring &strInput)
	{
		pstrOutput->clear();
		pstrOutput->reserve(strInput.length());

		std::transform(strInput.begin(), strInput.end(), std::back_inserter< std::wstring >(*pstrOutput), WideCharLower);
	}

	inline void ToUpper(std::wstring *pstr)
	{
		std::transform(pstr->begin(), pstr->end(), pstr->begin(), WideCharUpper);
	}

	inline void ToLower(std::wstring *pstr)
	{
		std::transform(pstr->begin(), pstr->end(), pstr->begin(), WideCharLower);
	}
/*
	inline void ToUpper(wchar_t *sz)
	{
		while(*sz)
		{
			*sz = LeoHelpers::WideCharUpper(*sz);
			++sz;
		}
	}

	inline void ToLower(wchar_t *sz)
	{
		while(*sz)
		{
			*sz = LeoHelpers::WideCharLower(*sz);
			++sz;
		}
	}
*/
	// Initialisation lists are about the only place it makes sense to use this.
	inline const std::wstring ToUpperTransientString(const wchar_t *szIn)
	{
		std::wstring strOut;
		LeoHelpers::ToUpper(&strOut, szIn);
		return strOut;
	}

	// Initialisation lists are about the only place it makes sense to use this.
	inline const std::wstring ToLowerTransientString(const wchar_t *szIn)
	{
		std::wstring strOut;
		LeoHelpers::ToLower(&strOut, szIn);
		return strOut;
	}

	inline bool IsAllAscii(const wchar_t *sz, bool bEmptyOkay)
	{
		if (!*sz)
		{
			return bEmptyOkay;
		}

		while(*sz)
		{
			if (!iswascii(*sz))
			{
				return false;
			}
			++sz;
		}

		return true;
	}

	inline bool IsAllDigits(const wchar_t *sz, bool bEmptyOkay)
	{
		if (!*sz)
		{
			return bEmptyOkay;
		}

		while(*sz)
		{
			if (!isdigit(*sz))
			{
				return false;
			}
			++sz;
		}

		return true;
	}

	inline bool IsAscii(unsigned char c) // This exists just to cast to unsigned char for the dangerously stupid CRT function.
	{
		return isascii(c) ? true : false; // Can't call ::isascii as isascii is a macro.
	}

	inline bool IsDigit(unsigned char c) // This exists just to cast to unsigned char for the dangerously stupid CRT function.
	{
		return ::isdigit(c) ? true : false;
	}

	inline bool IsPrefix(const wchar_t *sz, const wchar_t *prefix)
	{
		return 0 == wcsncmp(sz, prefix, wcslen(prefix));
	}

	inline bool IsPrefix(const std::wstring &str, const std::wstring &prefix)
	{
		return 0 == wcsncmp(str.c_str(), prefix.c_str(), prefix.length());
	}

	inline bool IsPrefixUpper(const wchar_t *sz, const wchar_t *prefix)
	{
		return 0 == WideStrCmpUpperLen(sz, prefix, wcslen(prefix));
	}

	inline bool IsPrefixUpper(const std::wstring &str, const std::wstring &prefix)
	{
		return 0 == WideStrCmpUpperLen(str, prefix, prefix.length());
	}

	inline bool IsSuffix(const wchar_t *string, const wchar_t *suffix)
	{
		size_t lenString = wcslen(string);
		size_t lenSuffix = wcslen(suffix);

		if (lenString < lenSuffix) return false;

		return 0 == wcscmp(string + (lenString - lenSuffix), suffix);
	}

	inline bool IsSuffix(const std::wstring &string, const std::wstring &suffix)
	{
		size_t lenString = string.length();
		size_t lenSuffix = suffix.length();

		if (lenString < lenSuffix) return false;

		return 0 == wcscmp(string.c_str() + (lenString - lenSuffix), suffix.c_str());
	}

	inline bool IsSuffixUpper(const wchar_t *string, const wchar_t *suffix)
	{
		size_t lenString = wcslen(string);
		size_t lenSuffix = wcslen(suffix);

		if (lenString < lenSuffix) return false;

		return 0 == WideStrCmpUpper(string + (lenString - lenSuffix), suffix);
	}

	inline bool IsSuffixUpper(const std::wstring &string, const std::wstring &suffix)
	{
		size_t lenString = string.length();
		size_t lenSuffix = suffix.length();

		if (lenString < lenSuffix) return false;

		return 0 == WideStrCmpUpper(string.c_str() + (lenString - lenSuffix), suffix.c_str());
	}

	std::wstring StringTrim(const std::wstring &strIn, const wchar_t *szTrimChars, bool bTrimLeft, bool bTrimRight);

	inline void StringCopy(wchar_t *dest, const wchar_t *source, size_t destSizeChars)
	{
		if (destSizeChars > 0 && dest != NULL)
		{
			wcsncpy_s(dest, destSizeChars, source, _TRUNCATE);
		}
	}

	inline void StringCopy(wchar_t *dest, size_t destSizeChars, const wchar_t *source)
	{
		if (destSizeChars > 0 && dest != NULL)
		{
			wcsncpy_s(dest, destSizeChars, source, _TRUNCATE);
		}
	}

	// Unlike strncat, destSizeChars should be the total size of dest, not the remaining size.
	inline void StringAppend(wchar_t *dest, const wchar_t *source, size_t destSizeChars)
	{
		const size_t destLen = wcslen(dest);
		dest += destLen;
		LeoHelpers::StringCopy(dest, source, destSizeChars - destLen);
	}

	// Returns the index of the null terminator. -1 on error.
	inline int VStringFormat(wchar_t *buffer, size_t bufferSizeChars, const wchar_t *format, va_list args)
	{
		int result = 0;

		if (bufferSizeChars > 0)
		{
			result = _vsnwprintf_s(buffer, bufferSizeChars, _TRUNCATE, format, args);
		}

		return(result);
	}

	// Returns the index of the null terminator. -1 on error.
	inline int StringFormat(wchar_t *buffer, size_t bufferSizeChars, const wchar_t *format, ...)
	{
		va_list args;
		va_start(args, format);
		int result = LeoHelpers::VStringFormat(buffer, bufferSizeChars, format, args);
		va_end(args);

		return(result);
	}

	// delete[] the result.
	// Returns NULL on failure.
	wchar_t *VStringAllocAndFormat(const wchar_t *format, va_list args);

	// delete[] the result.
	// Returns NULL on failure.
	inline wchar_t *StringAllocAndFormat(const wchar_t *format, ...)
	{
		va_list args;
		va_start(args, format);
		wchar_t *result = LeoHelpers::VStringAllocAndFormat(format, args);
		va_end(args);

		return(result);
	}

	// delete[] the result.
	// Returns NULL on failure.
	inline wchar_t *StringAllocAndCopy(const wchar_t *sz)
	{
		if (sz == NULL) { return NULL; }
		size_t len = wcslen(sz)+1;
		wchar_t *szResult = new(std::nothrow) wchar_t[len];
		if (szResult == NULL) { return NULL; }
		StringCopy(szResult, sz, len);
		return szResult;
	}

	// These are less efficient, since they copy the string an extra time, but safer and easier.
	// Returns boolean success.
	bool StringFormatToString(std::wstring *pResult, const wchar_t *format, ...);
	// Returns boolean success.
	bool VStringFormatToString(std::wstring *pResult, const wchar_t *format, va_list args);

	// delete[] the result.
	// Returns NULL on failure.
	// *pdwLengthChars will be set to the size of the returned buffer, including all the null terminators.
	// If the vector is empty then, technically, only a single null should be returned. However, some things are buggy
	// and look for a double-null terminator without considering what happens in the empty-list case. Because of them,
	// an empty list will generate a string with two nulls, but the *pLengthChars will still be set to 1. So you can
	// safely pass the result to buggy code but if you copy the buffer and want the copy to be safe for buggy code as well
	// then you might want to add an extra null in the empty case.
	wchar_t *MultiStringFromVector(const std::vector< std::wstring > *pVecStrings, size_t *pLengthChars);

	// szInput -- String to parse.
	// delimiters -- Characters which delimit the string.
	// terminators -- Characters which end the string. Can be NULL.
	// results -- A string vector, list or deque to put the results into. Existing contents will be erased. Give NULL if you don't want any results.
	// bSkipEmpty -- Set true to skip any empty strings. (e.g. someone typed "blah,,blah")
	// bQuotes -- Set to true to handle double-quotes (") around items. A quoted empty string is NOT skipped even if bSkipEmpty is set. Terminators in quotes are ignored and included in the tokenized strings.
	// bCSVQuotes -- (Ignored if bQuotes false) -- Inside quoted strings, two double-quotes next to each other will be output as one double-quote and not signify the end of the string. A string like "Blah""blah" will become blah"blah.
	// Leading and trailing spaces are always stripped.
	// Returns false iff nothing could be extracted (e.g. szInput was NULL)
	template< class Container > bool Tokenize(const wchar_t *szInput, const wchar_t *delimiters, const wchar_t *terminators, Container *pResults, bool bSkipEmpty, bool bQuotes, bool bCSVQuotes)
	{
		if (NULL != pResults)
		{
			pResults->clear();
		}

		bool bResult = false;

		size_t inLen = wcslen(szInput) + 1;
		wchar_t *szBuff = new(std::nothrow) wchar_t[inLen];

		if (szBuff != NULL)
		{
			while (LeoHelpers::Tokenize(&szInput, delimiters, terminators, szBuff, inLen, bSkipEmpty, bQuotes, bCSVQuotes))
			{
				bResult = true;

				if (NULL != pResults)
				{
					pResults->push_back(szBuff);
				}
			}

			delete[] szBuff;
		}

		return bResult;
	}

	// pp -- Pointer to buffer to tokenize. Will be advanced to the next position string. Set to NULL after the last token is extracted.
	// delimiters -- Characters which delimit the string.
	// terminators -- Characters which end the string. Can be NULL.
	// bSkipEmpty -- Set true to skip any empty strings. (e.g. someone typed "blah,,blah")
	// bQuotes -- Set to true to handle double-quotes (") around items. A quoted empty string is NOT skipped even if bSkipEmpty is set.
	// bCSVQuotes -- (Ignored if bQuotes false) -- Inside quoted strings, two double-quotes next to each other will be output as one double-quote and not signify the end of the string. A string like "Blah""blah" will become blah"blah.
	// buf -- Buffer to extract in to. Set to NULL not to extract.
	// bufSize -- Size of buf if it is non-NULL. Will truncate string if buf too small.
	// Leading and trailing spaces are always stripped.
	// Returns false iff nothing could be extracted (i.e. *pp was NULL)
	// pp and the contents of buf are only valid after calling if true is returns.
	// Lame function should allocate us a suitably sized buffer for each string... :-)
	// But hey, this isn't a public OS function and you can just give a buffer the size of the input...
	bool Tokenize(const wchar_t **pp, const wchar_t *delimiters, const wchar_t *terminators, wchar_t *buf, size_t bufSize, bool bSkipEmpty, bool bQuotes, bool bCSVQuotes);

	// Returns 0 on failure (use GetLastError()), else the number of wide characters written.
	// delete [] *pwcStringResult when finished with it.
	// numInputBytes -1 for null-terminated string.
	int MBtoWC(wchar_t **pwcStringResult, const char *mbStringInput, int numInputBytes, DWORD dwCodePage);

	// Returns 0 on failure (use GetLastError()), else the number of characters written.
	// delete [] *pmbStringResult when finished with it.
	// numInputWords -1 for null-terminated string.
	int WCtoMB(char **pmbStringResult, const wchar_t *wcStringInput, int numInputWords, DWORD dwCodePage);

	inline int MBtoWC(std::wstring *pstrStringResult, const char *mbStringInput, int numInputBytes, DWORD dwCodePage)
	{
		wchar_t *sz = NULL;

		int iResult = LeoHelpers::MBtoWC(&sz, mbStringInput, numInputBytes, dwCodePage);

		if (0 >= iResult)
		{
			pstrStringResult->clear();
		}
		else
		{
			*pstrStringResult = sz;
		}

		delete[] sz;

		return iResult;
	}

	inline int WCtoMB(std::string *pstrStringResult, const wchar_t *wcStringInput, int numInputWords, DWORD dwCodePage)
	{
		char *sz = NULL;

		int iResult = LeoHelpers::WCtoMB(&sz, wcStringInput, numInputWords, dwCodePage);

		if (0 >= iResult)
		{
			pstrStringResult->clear();
		}
		else
		{
			*pstrStringResult = sz;
		}

		delete[] sz;

		return iResult;
	}

	// Returns NULL on failure (use GetLastError())
	// delete[] the result when finished with it.
	inline wchar_t *MBtoWC(const char *mbString)
	{
		wchar_t *result = NULL;
		LeoHelpers::MBtoWC(&result, mbString, -1, CP_ACP);
		return(result);
	}

	// Returns NULL on failure (use GetLastError())
	// delete[] the result when finished with it.
	inline char *WCtoMB(const wchar_t *wcString)
	{
		char *result = NULL;
		LeoHelpers::WCtoMB(&result, wcString, -1, CP_ACP);
		return(result);
	}

	inline bool MBtoWC(std::wstring *pOutString, const char *mbString)
	{
		 wchar_t *t = LeoHelpers::MBtoWC(mbString);

		 if (!t)
		 {
			 pOutString->clear();
			 return false;
		 }

		 *pOutString = t;
		 delete[] t;

		 return true;
	}

	inline bool WCtoMB(std::string *pOutString, const wchar_t *wcString)
	{
		 char *t = LeoHelpers::WCtoMB(wcString);

		 if (!t)
		 {
			 pOutString->clear();
			 return false;
		 }

		 *pOutString = t;
		 delete[] t;

		 return true;
	}

	inline bool IsUtf16LeftSurrogate(wchar_t c)
	{
		// The casts might help if wchar_t is signed. :)
		return (((unsigned short)c) >= ((unsigned short)0xD800) && ((unsigned short)c) <= ((unsigned short)0xDBFF));
	}

	// Returns the new null position.
	size_t TruncateString(wchar_t *szBuffer, size_t idxNull, size_t cchSpaceAfterNull, bool bTruncateWithElipsis);

	bool MBtoWCInPlaceTruncate(const char *szSource, int cbInputBytes, wchar_t *szDestBuffer, int cchBufferSize, DWORD dwCodePage, bool bTruncateWithElipsis, bool *pbWasTruncated);
};
