#pragma once

// Macros suck but I need constant expressions for switch statements. :-(

#define MAKE64BITVERSIONNUMBER(a,b,c,d) ((((DWORD64)(a))<<48) + (((DWORD64)(b))<<32) + (((DWORD64)(c))<<16) + (((DWORD64)(d))<<00))
#define MAKE_DWORD_NAME(a,b,c,d) (((DWORD)(unsigned char)(d)<<24)+((DWORD)(unsigned char)(c)<<16)+((DWORD)(unsigned char)(b)<<8)+((DWORD)(unsigned char)(a)))


namespace LeoHelpers
{
	bool IsWindowsVersionGreaterOrEqual(DWORD dwMaj, DWORD dwMin, WORD wSPMaj, WORD wSPMin);

	inline bool IsWindowsXPOrAbove()
	{
		// 5.0.0.0 would mean Windows Server 2003, Windows XP, or Windows 2000
		// 5.1.0.0 means Windows XP
		return IsWindowsVersionGreaterOrEqual(5,1,0,0);
	}

	inline bool IsWindowsVistaOrAbove()
	{
		// 6.0.0.0 means Windows Vista or Windows Server 2008
		return IsWindowsVersionGreaterOrEqual(6,0,0,0);
	}

	inline bool IsWindows7OrAbove()
	{
		// 6.1.0.0 means Windows 7 or Windows Server 2008 R2
		return IsWindowsVersionGreaterOrEqual(6,1,0,0);
	}

	inline bool IsWow64ProcessWrapper(HANDLE hProc)
	{
		typedef BOOL (WINAPI *PFN_ISWOW64PROCESS)(__in HANDLE hProcess, __out PBOOL Wow64Process);

		PFN_ISWOW64PROCESS pfn_isWow64Process = reinterpret_cast< PFN_ISWOW64PROCESS >(
			::GetProcAddress(::GetModuleHandle(L"kernel32.dll"), "IsWow64Process"));

		if (pfn_isWow64Process==NULL)
		{
			return false; // Can't be a Wow64 process if there's no IsWow64Process on this OS.
		}

		if (hProc == NULL)
		{
			hProc = ::GetCurrentProcess();
		}

		BOOL bIsWow64 = FALSE;

		return (pfn_isWow64Process(hProc, &bIsWow64) && bIsWow64);
	}

	inline bool Is64BitWindows()
	{
#if defined(_WIN64)
		return true;
#elif defined(_WIN32)
		return LeoHelpers::IsWow64ProcessWrapper(NULL);
#else
#error Neither Win32 nor Win64.
#endif
	}

	inline bool Is64BitProcess()
	{
#if defined(_WIN64)
		return true;
#elif defined(_WIN32)
		return false;
#else
#error Neither Win32 nor Win64.
#endif
	}


	inline bool Is32BitProcess()
	{
#if defined(_WIN64)
		return false;
#elif defined(_WIN32)
		return true;
#else
#error Neither Win32 nor Win64.
#endif
	}

	inline bool Is32BitProcessOn64BitWindows()
	{
		return (LeoHelpers::Is32BitProcess() && LeoHelpers::Is64BitWindows());
	}

	bool WantWindowAnimations();
	bool WantFullWindowDragging();

	inline DWORD SwapDWORD(DWORD dw)
	{
		return ((dw<<24)&0xFF000000)
			 + ((dw<<8 )&0x00FF0000)
			 + ((dw>>8 )&0x0000FF00)
			 + ((dw>>24)&0x000000FF);
	}

	inline WORD SwapWORD(WORD w)
	{
		return ((w<<8 )&0xFF00)
			 + ((w>>8 )&0x00FF);
	}

	inline int MulDivRoundDown(int nNumber, int nNumerator, int nDenominator)
	{
		assert(nDenominator != 0);

		if (nDenominator == 0)
		{
			return -1; // Do what MulDiv does.
		}

		__int64 x = nNumber;
		x *= nNumerator;
		x /= nDenominator;

		assert(x <= INT_MAX && x >= INT_MIN);

		if (x > INT_MAX || x < INT_MIN)
		{
			return -1;
		}

		return static_cast< int >(x);
	}

	inline bool SetClipboard(HWND hwndClipboardOwner, const wchar_t *sz)
	{
		bool bResult = false;

		size_t cchBufLen = wcslen(sz) + 1;

		size_t cbBufSize = cchBufLen * sizeof(wchar_t);

		HGLOBAL hGlobalClipMem = ::GlobalAlloc(GMEM_MOVEABLE, cbBufSize);

		if (hGlobalClipMem != NULL)
		{
			wchar_t *pGlobalClipBuffer = reinterpret_cast<wchar_t *>( ::GlobalLock(hGlobalClipMem) );

			if (NULL != pGlobalClipBuffer)
			{
				LeoHelpers::StringCopy(pGlobalClipBuffer, sz, cchBufLen);

				::GlobalUnlock(hGlobalClipMem);

				if (::OpenClipboard(hwndClipboardOwner))
				{
					if (::EmptyClipboard())
					{
						if (NULL != ::SetClipboardData(CF_UNICODETEXT, hGlobalClipMem))
						{
							hGlobalClipMem = NULL; // Clipboard owns it now.
							bResult = true;
						}
					}

					if (!::CloseClipboard())
					{
						bResult = false;
					}
				}
			}

			if (hGlobalClipMem != NULL)
			{
				::GlobalFree(hGlobalClipMem);
				hGlobalClipMem = NULL;
			}
		}

		return bResult;
	}

	inline bool GetClipboard(HWND hwndClipboardOwner, std::wstring *pstrOut, bool bPrefixUTF16BOM, bool *pbOpenFailed)
	{
		bool bResult = false;

		if (pbOpenFailed)
		{
			*pbOpenFailed = false;
		}

		if (!::OpenClipboard(hwndClipboardOwner))
		{
			if (pbOpenFailed)
			{
				*pbOpenFailed = true;
			}
		}
		else
		{
			if (::IsClipboardFormatAvailable(CF_UNICODETEXT))
			{
				HANDLE hGlobalClipMem = ::GetClipboardData(CF_UNICODETEXT);

				if (hGlobalClipMem != NULL)
				{
					const wchar_t *szClipboard = reinterpret_cast<const wchar_t *>( ::GlobalLock(hGlobalClipMem) );

					if (szClipboard)
					{
						const wchar_t szIntelBom[2] = { 0xFEFF, 0 };

						try // We really don't want an exception from std::wstring to stop us closing the clipboard.
						{
							if (bPrefixUTF16BOM && szClipboard[0] != szIntelBom[0])
							{
								*pstrOut += szIntelBom;
							}

							*pstrOut += szClipboard;
							bResult = true;
						}
						catch(...)
						{
						}

						::GlobalUnlock(hGlobalClipMem);
					}
				}
			}

			::CloseClipboard();
		}

		if (!bResult)
		{
			pstrOut->clear();
		}

		return bResult;
	}

	// This requires both a section and a key name. It won't get you a list of names like the real GetPrivateProfileString.
	// Blank values will never be returned and are considered errors.
	inline bool GetPrivateProfileString(std::wstring *pstrResult, const wchar_t *szFileName, const wchar_t *szSectionName, const wchar_t *szKeyName)
	{
		if (szFileName    == NULL || szFileName[0]    == L'\0'
		||	szSectionName == NULL || szSectionName[0] == L'\0'
		||	szKeyName     == NULL || szKeyName[0]     == L'\0')
		{
			return false;
		}

		bool bResult = false;

		DWORD dwBufferSize = 512;
		wchar_t *szBuffer = new(std::nothrow) wchar_t[dwBufferSize];

		while(true)
		{
			DWORD dwRes = ::GetPrivateProfileString(szSectionName, szKeyName, L"", szBuffer, dwBufferSize, szFileName);

			if (dwRes == (dwBufferSize - 1))
			{
				delete [] szBuffer;
				dwBufferSize *= 2;
				szBuffer = new(std::nothrow) wchar_t[dwBufferSize];
			}
			else
			{
				if (dwRes != 0)
				{
					*pstrResult = szBuffer;
					bResult = true;
				}

				break;
			}
		}

		delete [] szBuffer;

		return bResult;
	}

#ifdef LEOHELPERS_SCOPE_INCLUDED
	// Thread safe.
	// This loads strings using the Win32 ::LoadString.
	// There's a separate OpusStringLoader that is similar but loads strings from Directory Opus resources.
	class StringLoader
	{
	private:
		mutable LeoHelpers::CritSecWrap m_csw;
		HINSTANCE m_hInstance;
		// Map is to pointers rather than strings so that it doesn't matter if the map nodes move around in memory.
		std::map< UINT, std::wstring * > m_mapStrings;
	public:
		StringLoader(HINSTANCE hInstance) : m_hInstance(hInstance)
		{
		}

		~StringLoader()
		{
			for (std::map< UINT, std::wstring * >::iterator miter = m_mapStrings.begin(); miter != m_mapStrings.end(); ++miter)
			{
				std::wstring *pwstr = miter->second;
				delete pwstr;
			}
		}

		const wchar_t *Get(UINT id)
		{
			LeoHelpers::CriticalSectionScoper css(m_csw);

			std::wstring *&pwstr = m_mapStrings[id];

			if (pwstr == NULL)
			{
				wchar_t szBuffer[1024]; // Assume 1KB is enough for all our strings.

				int res = ::LoadString(m_hInstance, id, szBuffer, _countof(szBuffer)-1);

				if (res <= 0 || res >= _countof(szBuffer))
				{
					assert(false);
					return L"<missing string>";
				}

				szBuffer[res] = L'\0'; // Ensure the buffer is null-terminated. MSDN is unclear about what happens if the string is the same as the buffer length (i.e. no room for the null terminator) which is why we subtracted 1 from the size above.

				pwstr = new std::wstring(szBuffer); // This is the map's pointer.
			}

			return pwstr->c_str();
		}
	};
#endif // LEOHELPERS_SCOPE_INCLUDED

#ifdef LEOHELPERS_SCOPE_INCLUDED
	// Thread safe.
	// This class can be useful when you have to store a lot of strings which will mostly be the same.
	// (e.g. The compression method name of a set of files in an archive.)
	// It lets you store an index instead of the string, with only two copies of the string being stored in memory
	// instead of one copy per instance of it.
	class StringCache
	{
	private:
		mutable LeoHelpers::CritSecWrap m_csw;
		StringCache(const StringCache &rhs); //disallow
		StringCache &operator=(const StringCache &rhs); // disallow

		std::vector< std::wstring > m_vecStrings;
		std::map< std::wstring, DWORD > m_mapStrings;

	public:
		StringCache() { }
		~StringCache() { } // non-virtual

		ULONG Insert(const std::wstring &str)
		{
			LeoHelpers::CriticalSectionScoper css(m_csw);

			if (m_vecStrings.size() >= (ULONG_MAX-1))
			{
				return ULONG_MAX;
			}

			ULONG ulNewIdx = m_mapStrings.insert( std::pair< std::wstring, ULONG >( str, static_cast<ULONG>(m_vecStrings.size()) ) ).first->second;

			if (ulNewIdx == m_vecStrings.size()) // If not then it was already in.
			{
				m_vecStrings.push_back(str);
			}

			return ulNewIdx;
		}

		bool Find(std::wstring *pStrOut, ULONG ulId) const
		{
			LeoHelpers::CriticalSectionScoper css(m_csw);

			if (ulId < 0 || ulId >= m_vecStrings.size())
			{
				pStrOut->clear();
				return false;
			}

			*pStrOut = m_vecStrings[ulId];
			return true;
		}
	};
#endif // LEOHELPERS_SCOPE_INCLUDED

	// NOT THREAD SAFE.
	class SimpleTimeCheck
	{
	private:
		DWORD m_dwStartClock;

	public:
		// Zero has special meaning to us (means we are "reset" or were never started)
		// so bump the time if we happen to call on a multiple of 49.7 days' uptime.
		// Note that this zero business is unrelated to the issues with GetTickCount
		// wrapping every 49.7 days. We won't be able to measure time durations that long
		// and this class is only intended for short durations. The class *does* correctly
		// measure short durations which happen to cross one wrap-around period, though.
		static DWORD GetTickCountNotZero()
		{
			DWORD dwTC = ::GetTickCount();
			if (dwTC != 0) { return dwTC; }
			return 1;
		}

		SimpleTimeCheck(bool fStart) : m_dwStartClock(fStart ? GetTickCountNotZero() : 0) { }
		~SimpleTimeCheck() { } // NOT virtual
		SimpleTimeCheck(const SimpleTimeCheck &rhs)            { m_dwStartClock = rhs.m_dwStartClock; }
		SimpleTimeCheck &operator=(const SimpleTimeCheck &rhs) { m_dwStartClock = rhs.m_dwStartClock; return *this; }

		void Start()                                         { m_dwStartClock = GetTickCountNotZero(); }
		void Reset()                                         { m_dwStartClock = 0; }
		bool IsReset()                                 const { return (m_dwStartClock == 0); }
		DWORD GetDuration()                            const { if (IsReset()) { return 0; } return (GetTickCountNotZero() - m_dwStartClock); }
		DWORD GetTimeUntil(DWORD dwMilliseconds)       const { if (IsReset()) { return 0; } DWORD dwDur = GetDuration(); if (dwMilliseconds <= dwDur) { return 0; } return (dwMilliseconds - dwDur); }
		bool IsPastAndNotReset(  DWORD dwMilliseconds) const { return (!IsReset() && (GetTickCountNotZero() - m_dwStartClock) >= dwMilliseconds); }
		bool IsPastOrReset(      DWORD dwMilliseconds) const { return ( IsReset() || (GetTickCountNotZero() - m_dwStartClock) >= dwMilliseconds); }
		bool IsBeforeAndNotReset(DWORD dwMilliseconds) const { return (!IsReset() && (GetTickCountNotZero() - m_dwStartClock) <  dwMilliseconds); }
		bool IsBeforeOrReset(    DWORD dwMilliseconds) const { return ( IsReset() || (GetTickCountNotZero() - m_dwStartClock) <  dwMilliseconds); }
	};
};
