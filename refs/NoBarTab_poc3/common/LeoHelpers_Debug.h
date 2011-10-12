#pragma once

#ifdef _DEBUG
#define leoAssert(_Expression) (void)( (!!(_Expression)) || (LeoHelpers::LeoAssert::Show(_CRT_WIDE(#_Expression), _CRT_WIDE(__FILE__), __LINE__), 0) )
#else
#define leoAssert(_Expression) ((void)0)
#endif // _DEBUG

namespace LeoHelpers
{
	void OutputDebug(const wchar_t *szPrefix, const wchar_t *szClass, const wchar_t *szMethod, const wchar_t *szMessage);

	inline void VOutputDebugFormat(const wchar_t *szPrefix, const wchar_t *szClass, const wchar_t *szMethod, const wchar_t *szFormat, va_list args)
	{
		wchar_t *szMainMsg = LeoHelpers::VStringAllocAndFormat(szFormat ? szFormat : L"", args);

		if (szMainMsg != NULL)
		{
			LeoHelpers::OutputDebug(szPrefix, szClass, szMethod, szMainMsg);

			delete[] szMainMsg;
		}
	}

	inline void OutputDebugFormat(const wchar_t *szPrefix, const wchar_t *szClass, const wchar_t *szMethod, const wchar_t *szFormat, ...)
	{
		va_list args;
		va_start(args, szFormat);

		LeoHelpers::VOutputDebugFormat(szPrefix, szClass, szMethod, szFormat, args);

		va_end(args);
	}


#ifdef _DEBUG

	// Like _wassert but it shows the dialog box on a private UI thread to prevent the current thread running a message loop which then
	// processes messages and makes things go haywire (or just confusing to debug, like trigging follow-on asserts-within-asserts).
	class LeoAssert
	{
	private:
		const wchar_t *m_szExpression;
		const wchar_t *m_szFile;
		int m_iLine;
		bool m_bBreak;

		LeoAssert(const LeoAssert &rhs); // disallow
		LeoAssert &operator=(const LeoAssert &rhs); // disallow

		LeoAssert(const wchar_t *szExpression, const wchar_t *szFile, int iLine)
			: m_szExpression(szExpression)
			, m_szFile(szFile)
			, m_iLine(iLine)
			, m_bBreak(false)
		{
		}

		static unsigned __stdcall staticThread(void *pVoidThis);

	public:
		static void Show(const wchar_t *szExpression, const wchar_t *szFile, int iLine);
	};


	// Asserts if two threads use the class concurrently and/or if a single thread has made a re-entrant call.
	template < bool bCheckReentrancy, bool bCheckConcurrency > class ReConChecker
	{
	private:
		LeoHelpers::CritSecWrap m_csw;
		std::map< DWORD, int > m_mapThreadCount;
		int m_reentrancyAllows;

		ReConChecker(const ReConChecker &rhs); // disallow
		ReConChecker &operator=(const ReConChecker &rhs); // disallow

	public:
		ReConChecker() : m_reentrancyAllows(0)
		{
		}

		~ReConChecker() 
		{
			leoAssert(m_mapThreadCount.empty());
		}

		void AllowReentrancy()
		{
			LeoHelpers::CriticalSectionScoper css(m_csw);
			++m_reentrancyAllows;
		}

		void DisallowReentrancy()
		{
			LeoHelpers::CriticalSectionScoper css(m_csw);
			--m_reentrancyAllows;
		}

		void Enter()
		{
			int countThisThread = 0;
			int reentrancyAllows = 0;
			std::map< DWORD, int >::size_type numThreads = 0;

			{
				LeoHelpers::CriticalSectionScoper css(m_csw);
				countThisThread = ++m_mapThreadCount[ ::GetCurrentThreadId() ];
				numThreads = m_mapThreadCount.size();
				reentrancyAllows = m_reentrancyAllows;
			}

			if (bCheckConcurrency)                         { leoAssert(numThreads      == 1); }
			if (bCheckReentrancy && reentrancyAllows <= 0) { leoAssert(countThisThread == 1); }
		}

		void Exit()
		{
			bool bFound = false;
			bool bValid = false;

			{
				LeoHelpers::CriticalSectionScoper css(m_csw);
				std::map< DWORD, int >::iterator miter = m_mapThreadCount.find( ::GetCurrentThreadId() );
				
				if (bFound = (miter != m_mapThreadCount.end()))
				{
					if (bValid = (miter->second > 0))
					{
						if ( --(miter->second) == 0 )
						{
							m_mapThreadCount.erase(miter);
						}
					}
				}
			}

			leoAssert(bFound && bValid);
		}
	};


	template < bool bCheckReentrancy, bool bCheckConcurrency > class ReConScoper
	{
	private:
		ReConChecker< bCheckReentrancy, bCheckConcurrency > *m_pChecker;

		ReConScoper(const ReConScoper &rhs); // disallow
		ReConScoper &operator=(const ReConScoper &rhs); // disallow

	public:
		ReConScoper(ReConChecker< bCheckReentrancy, bCheckConcurrency > *pChecker)
		: m_pChecker(pChecker)
		{
			m_pChecker->Enter();
		}

		~ReConScoper()
		{
			m_pChecker->Exit();
			m_pChecker = NULL;
		}
	};

	template < bool bCheckReentrancy, bool bCheckConcurrency > class ReConReEnAllowScoper
	{
	private:
		ReConChecker< bCheckReentrancy, bCheckConcurrency > *m_pChecker;

		ReConReEnAllowScoper(const ReConReEnAllowScoper &rhs); // disallow
		ReConReEnAllowScoper &operator=(const ReConReEnAllowScoper &rhs); // disallow

	public:
		ReConReEnAllowScoper(ReConChecker< bCheckReentrancy, bCheckConcurrency > *pChecker)
		: m_pChecker(pChecker)
		{
			m_pChecker->AllowReentrancy();
		}

		~ReConReEnAllowScoper()
		{
			m_pChecker->DisallowReentrancy();
		}
	};

#endif // _DEBUG
};
