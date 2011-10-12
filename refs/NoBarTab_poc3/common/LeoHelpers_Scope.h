#pragma once

#define LEOHELPERS_SCOPE_INCLUDED

namespace LeoHelpers
{
	class CritSecWrap
	{
	private:
		CRITICAL_SECTION m_cs;
	private:
		CritSecWrap(const CritSecWrap &rhs); // disallow
		CritSecWrap &operator=(const CritSecWrap &rhs); // disallow
	public:
		CritSecWrap()                    { ::InitializeCriticalSection(&m_cs); }
		/* not virtual */ ~CritSecWrap() { ::DeleteCriticalSection(&m_cs);     }
	//	operator CRITICAL_SECTION * ()   { return &m_cs; }    <-- Not defining this because it'd invite mistaking the wrapper for normal CS and init/deleting it.
		CRITICAL_SECTION *GetCS()        { return &m_cs; } // <-- Instead, things like CriticalSectionScoper will understand how to use this as well.
	};

	class CriticalSectionScoper
	{
	private:
		CRITICAL_SECTION *m_pCS;
	private:
		CriticalSectionScoper(const CriticalSectionScoper &rhs); // disallow
		CriticalSectionScoper &operator=(const CriticalSectionScoper &rhs); // disallow
	public:
		CriticalSectionScoper(CRITICAL_SECTION *pCS) : m_pCS(pCS)    { EnterCriticalSection(m_pCS); }
		CriticalSectionScoper(CritSecWrap &csw) : m_pCS(csw.GetCS()) { EnterCriticalSection(m_pCS); }
		/*not virtual*/ ~CriticalSectionScoper()                     { LeaveCriticalSection(m_pCS); }
	};

	class CriticalSection_Try_Scoper
	{
	private:
		CRITICAL_SECTION *m_pCS;
		bool m_bSuccess;
	private:
		CriticalSection_Try_Scoper(const CriticalSection_Try_Scoper &rhs); // disallow
		CriticalSection_Try_Scoper &operator=(const CriticalSection_Try_Scoper &rhs); // disallow
	public:
		CriticalSection_Try_Scoper(CRITICAL_SECTION *pCS) : m_pCS(pCS),         m_bSuccess(false) { if (TryEnterCriticalSection(m_pCS)) { m_bSuccess = true; } }
		CriticalSection_Try_Scoper(CritSecWrap &csw)      : m_pCS(csw.GetCS()), m_bSuccess(false) { if (TryEnterCriticalSection(m_pCS)) { m_bSuccess = true; } }
		/*not virtual*/ ~CriticalSection_Try_Scoper() { if (m_bSuccess) { LeaveCriticalSection(m_pCS); } }
		bool Retry()             { if (!m_bSuccess && TryEnterCriticalSection(m_pCS)) { m_bSuccess = true; } return m_bSuccess; }
		void EnterIfNotAlready() { if (!m_bSuccess) { EnterCriticalSection(m_pCS); m_bSuccess = true; } }
		bool Succeeded() const { return  m_bSuccess; }
		bool Failed()    const { return !m_bSuccess; }
	};

	// This does the opposite of CriticalSectionScoper; i.e. leaves on construction and re-enters on destruction.
	// Obviously, you should only construct this object when you have entered the CS exactly once.
	class CriticalSectionScoperReverse
	{
	private:
		CRITICAL_SECTION *m_pCS;
	private:
		CriticalSectionScoperReverse(const CriticalSectionScoperReverse &rhs); // disallow
		CriticalSectionScoperReverse &operator=(const CriticalSectionScoperReverse &rhs); // disallow
	public:
		CriticalSectionScoperReverse(CRITICAL_SECTION *pCS) : m_pCS(pCS)    { LeaveCriticalSection(m_pCS); }
		CriticalSectionScoperReverse(CritSecWrap &csw) : m_pCS(csw.GetCS()) { LeaveCriticalSection(m_pCS); }
		/*not virtual*/ ~CriticalSectionScoperReverse()                     { EnterCriticalSection(m_pCS); }
	};

	template< typename T > class CriticalObject
	{
	private:
		mutable CRITICAL_SECTION m_cs;
	private:
		CriticalObject(const CriticalObject< T > &rhs); // disallow
		CriticalObject &operator=(const CriticalObject< T > &rhs); // disallow
	public:
		CriticalObject(T t) : object(t)   { InitializeCriticalSection(&m_cs); }
		/*not virtual*/ ~CriticalObject() { DeleteCriticalSection(&m_cs); }
		void Lock() const                 { EnterCriticalSection(&m_cs); }
		void Unlock() const               { LeaveCriticalSection(&m_cs); }
		T object;
	};

	template< typename T > class CriticalObjectScoper
	{
	private:
		const CriticalObject< T > *m_pCO;
	private:
		CriticalObjectScoper(const CriticalObjectScoper< T > &rhs); // disallow
		CriticalObjectScoper &operator=(const CriticalObjectScoper< T > &rhs); // disallow
	public:
		CriticalObjectScoper(const CriticalObject< T > *pCO) : m_pCO(pCO) { m_pCO->Lock(); }
		/*not virtual*/ ~CriticalObjectScoper()                           { m_pCO->Unlock(); }
	};

	class MutexWrapper
	{
	private:
		HANDLE m_hMutex;
	private:
		MutexWrapper(const MutexWrapper &rhs); // disallow
		MutexWrapper &operator=(const MutexWrapper &rhs); // disallow
	public:
		MutexWrapper(const wchar_t *szName) : m_hMutex(::CreateMutex(NULL, FALSE, szName)) { assert(m_hMutex != NULL); }
		/* not virtual */ ~MutexWrapper() { if (m_hMutex != NULL) { ::CloseHandle(m_hMutex); m_hMutex = NULL; } }
	//	operator HANDLE () { return m_hMutex; }    <-- Not defining this because it'd invite mistaking the wrapper for normal mutex and init/deleting it.
		HANDLE GetMX()     { return m_hMutex; } // <-- Instead, things like MutexScoper will understand how to use this as well.

	};

	class MutexScoper
	{
	private:
		HANDLE m_hMutex;
	private:
		MutexScoper(const MutexScoper &rhs); // disallow
		MutexScoper &operator=(const MutexScoper &rhs); // disallow
	public:
		MutexScoper(HANDLE hMutex)     : m_hMutex(hMutex)      { assert(m_hMutex != NULL); DWORD dwRes = ::WaitForSingleObject(m_hMutex, INFINITE); assert(dwRes == WAIT_OBJECT_0); }
		MutexScoper(MutexWrapper &mxw) : m_hMutex(mxw.GetMX()) { assert(m_hMutex != NULL); DWORD dwRes = ::WaitForSingleObject(m_hMutex, INFINITE); assert(dwRes == WAIT_OBJECT_0); }
		/*not virtual*/ ~MutexScoper()                         { ReleaseNow(); }
		void ReleaseNow()                                      { if (m_hMutex != NULL) { BOOL bRelMutexRes = ::ReleaseMutex(m_hMutex); m_hMutex = NULL; assert(bRelMutexRes); } }
	};

	class SemaphoreScoper
	{
	private:
		HANDLE m_hSem;
	private:
		SemaphoreScoper(const SemaphoreScoper &rhs); // disallow
		SemaphoreScoper &operator=(const SemaphoreScoper &rhs); // disallow
	public:
		SemaphoreScoper(HANDLE hSem) : m_hSem(hSem) { assert(hSem != NULL); DWORD dwRes = ::WaitForSingleObject(hSem, INFINITE); assert(dwRes == WAIT_OBJECT_0); }
		/*not virtual*/ ~SemaphoreScoper()          { BOOL bRelSemRes = ::ReleaseSemaphore(m_hSem, 1, NULL); assert(bRelSemRes); }
	};

	class SetEventOnDestruction
	{
	private:
		HANDLE m_hEvent;
	private:
		SetEventOnDestruction(const SetEventOnDestruction &rhs); //disallow
		SetEventOnDestruction &operator=(const SetEventOnDestruction &rhs); // disallow
	public:
		SetEventOnDestruction(HANDLE hEvent) : m_hEvent(hEvent) { }
		/* not virtual */ ~SetEventOnDestruction()              { if (m_hEvent != NULL) { ::SetEvent(m_hEvent); } }
	};

	class HandleScoper
	{
	private:
		HANDLE m_h;
		bool m_bHasHandle;
	private:
		HandleScoper(const HandleScoper &rhs); // disallow
		HandleScoper &operator=(const HandleScoper &rhs); // disallow
	public:
		HandleScoper()         : m_h(INVALID_HANDLE_VALUE), m_bHasHandle(false) { }
		HandleScoper(HANDLE h) : m_h(h),                    m_bHasHandle(true)  { }
		/* not virtual */ ~HandleScoper() { Close(); }
		inline bool   HasHandle() const   { return m_bHasHandle; }
		inline void   Forget()            { m_h = INVALID_HANDLE_VALUE; m_bHasHandle = false; }
		inline void   Close()             { if (m_bHasHandle) { ::CloseHandle(m_h); Forget(); } }
		inline HANDLE Get() const         { assert(m_bHasHandle); return m_h; }
		inline HANDLE GetAndForget()      { HANDLE h = m_h; Forget(); return h; }
		inline void   Set(HANDLE h)       { Close(); m_h = h; m_bHasHandle = true; }
	};

	// ReleaseScoper may be used on things which are not COM objects.
	// The only assumption it makes about T is that it has AddRef() and Release() methods.
	template< typename T > class ReleaseScoper
	{
	private:
		T *m_p;
	private:
		ReleaseScoper(const ReleaseScoper &rhs); // disallow
		ReleaseScoper &operator=(const ReleaseScoper &rhs); // disallow
	public:
		ReleaseScoper()     : m_p(NULL)            { }
		ReleaseScoper(bool bAddRef, T *p) : m_p(p) { if (bAddRef && m_p != NULL) { m_p->AddRef(); } }
		/* not virtual */ ~ReleaseScoper()         { ReleaseChild(); }
		inline bool IsEmpty() const                { return (m_p == NULL); }
		inline void Forget()                       { m_p = NULL; }
		inline void ReleaseChild()                 { if (m_p != NULL) { m_p->Release(); Forget(); } }
		inline const T * Get() const               { return m_p; }
		inline T * Get()                           { return m_p; }
		inline T * GetAndForget()                  { T *p = m_p; Forget(); return p; }
		inline void SetWithoutAddRef(T *p)         { ReleaseChild(); m_p = p; }
		inline void SetWithAddRef(T *p)            { if (p != NULL) { p->AddRef(); } SetWithoutAddRef(p); } // Correctly handles case where p is already what we store.

		inline bool QueryInterfaceFrom(IUnknown *pUnk, const IID &riid) // Any existing child will be released even if this fails.
		{
			// Correctly handles case where pUnk is m_p (i.e. we must not release it until after the QI call).

			T *p;

			if (pUnk == NULL || pUnk->QueryInterface(riid, reinterpret_cast< void ** >(&p)) != S_OK || p == NULL)
			{
				ReleaseChild();
				return false;
			}

			SetWithoutAddRef(p);
			return true;
		}

		inline bool QueryInterfaceFrom(IUnknown *pUnk) { return QueryInterfaceFrom(pUnk, __uuidof(T)); } // Any existing child will be released even if this fails.

		inline bool CreateInstance(const CLSID &clsid, const IID &riid) // Any existing child will be released (before the attempt) even if this fails.
		{
			ReleaseChild();

			T *p;
			
			if (::CoCreateInstance(clsid, NULL, CLSCTX_INPROC_SERVER|CLSCTX_LOCAL_SERVER, riid, reinterpret_cast< void ** >(&p)) != S_OK || p == NULL)
			{
				return false;
			}

			SetWithoutAddRef(p);
			return true;
		}

		inline bool CreateInstance(const CLSID &clsid) { return CreateInstance(clsid, __uuidof(T)); } // Any existing child will be released (before the attempt) even if this fails.
	};

	// Never pass DeleteScoper objects across process/DLL boundaries.
	// (It's all inline so the new/delete calls may hit different heaps.)
	// Do not use DeleteScoper with arrays that need delete[]; use ArrayScoper for those.
	template <typename T> class DeleteScoper
	{
	protected:
		T *m_p;
	private:
		DeleteScoper(const DeleteScoper &rhs); // disallow
		DeleteScoper &operator=(const DeleteScoper &rhs); // disallow
	public:
		DeleteScoper() : m_p(NULL)        { }
		DeleteScoper(T *p) : m_p(p)       { }
		/* not virtual */ ~DeleteScoper() { Free(); }
		inline bool     IsEmpty() const   { return (m_p == NULL); }
		inline void     Free()            { delete m_p; m_p = NULL; }
		inline void     Forget()          { m_p = NULL; }
		inline void     Set(T *p)         { if (p != m_p) { Free(); m_p = p; } }
		inline       T *Get()             { return m_p; }
		inline const T *Get() const       { return m_p; }
		inline       T *GetAndForget()    { T *p = m_p; Forget(); return p; }
	//	operator T*() { return m_p; }
	};

	// Never pass ArrayScoper objects across process/DLL boundaries.
	// (It's all inline so the new/delete calls may hit different heaps.)
	template <typename T> class ArrayScoper
	{
	protected:
		T *m_p;
	private:
		ArrayScoper(const ArrayScoper &rhs); // disallow
		ArrayScoper &operator=(const ArrayScoper &rhs); // disallow
	public:
		ArrayScoper() : m_p(NULL)        { }
		ArrayScoper(T *p) : m_p(p)       { }
		/* not virtual */ ~ArrayScoper() { Free(); }
		inline bool     IsEmpty() const  { return (m_p == NULL); }
		inline void     Free()           { delete[] m_p; m_p = NULL; }
		inline void     Forget()         { m_p = NULL; }
		inline void     Set(T *p)        { if (p != m_p) { Free(); m_p = p; } }
		inline       T *Get()            { return m_p; }
		inline const T *Get() const      { return m_p; }
		inline       T *GetAndForget()   { T *p = m_p; Forget(); return p; }
	//	operator T*() { return m_p; }
	};

	typedef ArrayScoper< wchar_t > WCharArrayScoper;

	// ArrayScoperDelOpt is like ArrayScoper but it can also be given static arrays that aren't for deletion.
	// Never pass ArrayScoperDelOpt objects across process/DLL boundaries.
	// (It's all inline so the new/delete calls may hit different heaps.)
	template <typename T> class ArrayScoperDelOpt
	{
	protected:
		T *m_p;
		bool m_bDelete;
	private:
		ArrayScoperDelOpt(const ArrayScoperDelOpt &rhs); // disallow
		ArrayScoperDelOpt &operator=(const ArrayScoperDelOpt &rhs); // disallow
	public:
		ArrayScoperDelOpt()                   : m_p(NULL), m_bDelete(false)   { }
		ArrayScoperDelOpt(bool bDelete, T *p) : m_p(p),    m_bDelete(bDelete) { }
		/* not virtual */ ~ArrayScoperDelOpt()         { Free(); }
		inline bool     IsEmpty() const                { return (m_p == NULL); }
		inline bool     IsGoignToDelete() const        { return m_bDelete; }
		inline void     Free()                         { if (m_bDelete) { delete[] m_p; } m_p = NULL; m_bDelete = false; }
		inline void     Forget()                       { m_p = NULL; m_bDelete = false; }
		inline void     Set(bool bDelete, T *p)        { if (p != m_p) { Free(); m_p = p; m_bDelete = bDelete; } else if (bDelete) { m_bDelete = true; } }
		inline       T *Get()            { return m_p; }
		inline const T *Get() const      { return m_p; }
		inline       T *GetAndForget()   { T *p = m_p; Forget(); return p; }
	//	operator T*() { return m_p; }
	};

	typedef ArrayScoperDelOpt< wchar_t > WCharArrayScoperDelOpt;

	// Never pass MultiArrayScoper objects across process/DLL boundaries.
	// (It's all inline so the new/delete calls may hit different heaps.)
	class MultiArrayScoper
	{
	private:
		std::vector< void * > m_voidPointers;
	private:
		MultiArrayScoper(const MultiArrayScoper &rhs); // disallow
		MultiArrayScoper &operator=(const MultiArrayScoper &rhs); // disallow
	public:
		MultiArrayScoper() { }
		/* not virtual */ ~MultiArrayScoper() { while(!m_voidPointers.empty()) { delete [] m_voidPointers.back(); m_voidPointers.pop_back(); } }
		void Add(void *pVoid) { m_voidPointers.push_back(pVoid); }
	};

	// Never pass AutoBuffer objects across process/DLL boundaries.
	// (It's all inline so the new/delete calls may hit different heaps.)
	template <typename T> class AutoBuffer
	{
	private:
		BYTE *m_pBuffer;
		size_t m_size;
	private:
		AutoBuffer(const AutoBuffer &rhs); // disallow
		AutoBuffer &operator=(const AutoBuffer &rhs); // disallow
	public:
		AutoBuffer() : m_pBuffer(0) , m_size(0) { }
		/* not virtual */ ~AutoBuffer() { Free(); } // Must not change Win32 LastError
		void Free() { delete[] m_pBuffer; m_pBuffer = 0; m_size = 0; } 		// Must not change Win32 LastError

		// Allocating a zero-length buffer is legal and results in a non-null buffer.
		// Make no assumptions about the content of the buffer after allocation.
		bool AllocateBytes(size_t s)
		{
			if (s == m_size && m_pBuffer != 0) { return true; }
			Free();
			m_pBuffer = new(std::nothrow) BYTE[s];
			if (m_pBuffer == 0) { m_size = 0; ::SetLastError(ERROR_NOT_ENOUGH_MEMORY); return false; }
			m_size = s;
			return true;
		}

		// If there's already a buffer big enough (or bigger) then do nothing, else (re-)allocate.
		bool AllocateMinimumBytes(size_t s)
		{
			if (s <= m_size && m_pBuffer != 0) { return true; }
			return AllocateBytes(s);
		}

		bool        AllocateElements(size_t c) {        return AllocateBytes(c * sizeof(T)); }
		bool AllocateMinimumElements(size_t c) { return AllocateMinimumBytes(c * sizeof(T)); }

		T *    GetBuffer()             { return reinterpret_cast<       T * >(m_pBuffer); }
		T *    GetBuffer()       const { return reinterpret_cast< const T * >(m_pBuffer); }
		size_t GetSizeBytes()    const { return m_size; }
		size_t GetSizeElements() const { return (m_size / sizeof(T)); }
		bool   IsAllocated()     const { return (m_pBuffer != 0); }
	};

	class LocalFreeScoper
	{
	private:
		HLOCAL m_h;
		bool m_bHasHandle;
	private:
		LocalFreeScoper(const LocalFreeScoper &rhs); // disallow
		LocalFreeScoper &operator=(const LocalFreeScoper &rhs); // disallow
	public:
		LocalFreeScoper()         : m_h(NULL), m_bHasHandle(false) { }
		LocalFreeScoper(HLOCAL h) : m_h(h),    m_bHasHandle(true)  { }
		/* not virtual */ ~LocalFreeScoper() { Free(); }
		inline bool HasHandle() const         { return m_bHasHandle; }
		inline void Forget()                  { m_h = NULL; m_bHasHandle = false; }
		inline void Free()                    { if (m_bHasHandle) { ::LocalFree(m_h); Forget(); } }
		inline HLOCAL Get() const             { assert(m_bHasHandle); return m_h; }
		inline HLOCAL GetAndForget()          { HLOCAL h = m_h; Forget(); return h; }
		inline void Set(HLOCAL h)             { if (h != m_h) { Free(); m_h = h; m_bHasHandle = true; } }
		inline bool AllocateFixed(SIZE_T uBytes)
		{
			Free();
			m_h = ::LocalAlloc(LMEM_FIXED, uBytes);
			if (m_h) { m_bHasHandle = true; }
			return m_bHasHandle;
		}
	};

	class BinaryData
	{
	protected:
		BYTE *m_pData;
		DWORD m_dwSize;
		bool  m_bUseLocalAlloc;

	public:
		BinaryData(bool bUseLocalAlloc)
		: m_pData(NULL)
		, m_dwSize(0)
		, m_bUseLocalAlloc(bUseLocalAlloc)
		{
		}

		~BinaryData()
		{
			Clear();
		}

	private:
		BinaryData(const BinaryData &rhs); // disallow
		BinaryData &operator=(const BinaryData &rhs); // disallow

	public:

		void Clear()
		{
			if (m_pData != NULL)
			{
				if (m_bUseLocalAlloc)
				{
					::LocalFree(m_pData);
				}
				else
				{
					delete[] m_pData;
				}
			}
			m_pData = NULL;
			m_dwSize = 0;
		}

		void Forget()
		{
			m_pData = NULL;
			m_dwSize = 0;
		}

		void SetAndOwn(BYTE *pData, DWORD dwSize, bool bUseLocalAlloc)
		{
			Clear();

			m_pData = pData;
			m_dwSize = dwSize;
			m_bUseLocalAlloc = bUseLocalAlloc;
		}

		bool Load(const wchar_t *filename, DWORD dwMaxRead=INVALID_FILE_SIZE)
		{
			bool bResult = false;

			Clear();

			HANDLE hFile = ::CreateFile(filename, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);

			if (INVALID_HANDLE_VALUE == hFile)
			{
			}
			else
			{
				DWORD dwFileSize = ::GetFileSize(hFile, NULL);

				if (INVALID_FILE_SIZE == dwFileSize)
				{
				}
				else if (0 == dwFileSize)
				{
				}
				else
				{
					if (dwMaxRead != INVALID_FILE_SIZE && dwFileSize > dwMaxRead)
					{
						dwFileSize = dwMaxRead;
					}

					if (m_bUseLocalAlloc)
					{
						m_pData = reinterpret_cast< BYTE *>( ::LocalAlloc(LMEM_FIXED, dwFileSize) );
					}
					else
					{
						m_pData = new(std::nothrow) BYTE[dwFileSize];
					}

					if (m_pData == NULL)
					{
					}
					else
					{
						if (0 == ::ReadFile(hFile, m_pData, dwFileSize, &m_dwSize, NULL) || m_dwSize != dwFileSize)
						{
						}
						else
						{
							bResult = true;
						}
					}
				}

				::CloseHandle(hFile);
			}

			if (!bResult)
			{
				Clear();
			}

			return bResult;
		}

		/*
		bool Save(const wchar_t *filename)
		{
			return Save(filename, m_pData, m_dwSize);
		}

		static bool Save(const wchar_t *filename, const BYTE *pData, DWORD dwSize)
		{
			bool bResult = false;

			HANDLE hFile = ::CreateFile(filename, FILE_WRITE_DATA, FILE_SHARE_READ, NULL, CREATE_NEW, FILE_FLAG_SEQUENTIAL_SCAN, NULL);

			if (hFile == INVALID_HANDLE_VALUE)
			{
				DWORD dwErr = GetLastError();

				if (dwErr == ERROR_ALREADY_EXISTS
				||	dwErr == ERROR_FILE_EXISTS)
				{
					wcprintf(L"Failed to save (already exists): %s\n", filename);
				}
				else
				{
					wcprintf(L"Failed to save (error %ld): %s\n", dwErr, filename);
				}
			}
			else
			{
				DWORD dwWritten = 0;

				if (!WriteFile(hFile, pData, dwSize, &dwWritten, NULL))
				{
					DWORD dwErr = GetLastError();
					wcprintf(L"Failed to write (error %ld): %s\n", dwErr, filename);
				}
				else if (dwWritten != dwSize)
				{
					wcprintf(L"Failed to write (only partially written): %s\n", filename);
				}
				else
				{
					bResult = true;
				}

				CloseHandle(hFile);

				if (!bResult)
				{
					DeleteFile(filename);
				}
			}

			return bResult;
		}
		*/

		BYTE *GetBuffer()
		{
			return m_pData;
		}

		DWORD GetSize()
		{
			return m_dwSize;
		}
	};

	// Ref-counted ID object which can outlive the parent object it represents. Similar to comparing the address
	// of a the parent object instance but without the problem that if that parent object is destroyed and another
	// created, the new one may have the same address as the old and look like the same object to anything that
	// compared a stored (now dangling/recycled) pointer. Since the ID object is ref-counted, its address won't be
	// recycled until all interested parties have released it. This also removes problems with circular references
	// since the InstanceIdent itself never references any other object.
	class InstanceIdent
	{
	private:
		volatile LONG m_lRefCount;

		InstanceIdent(const InstanceIdent &rhs); // disallow
		InstanceIdent &operator=(const InstanceIdent &rhs); // disallow

		// Destructor private as destruction must be through Release.
		~InstanceIdent() { } // Non-virtual.
	public:
		InstanceIdent() : m_lRefCount(1) { }
		ULONG AddRef()  { return InterlockedIncrement(&m_lRefCount); }
		ULONG Release() { LONG lRefCount = InterlockedDecrement(&m_lRefCount); if (lRefCount==0) { delete this; } return lRefCount; }

	//	bool operator==(const InstanceIdent &rhs)     const { return (this==&rhs); }
	//	bool operator!=(const InstanceIdent &rhs)     const { return (this!=&rhs); }
	//	bool IsEqualTo(const InstanceIdent &rhs)      const { return (this==&rhs); }
		bool IsEqualTo(const InstanceIdent *pRhs)     const { return (this==pRhs); }
	//	bool IsDifferentTo(const InstanceIdent &rhs)  const { return (this!=&rhs); }
		bool IsDifferentTo(const InstanceIdent *pRhs) const { return (this!=pRhs); }
	};

	class EnvBlockWideScoper
	{
	private:
		LPWCH m_envStringsW;
	private:
		EnvBlockWideScoper(const EnvBlockWideScoper &rhs); // disallow
		EnvBlockWideScoper &operator=(const EnvBlockWideScoper &rhs); // disallow
	public:
		EnvBlockWideScoper() : m_envStringsW(::GetEnvironmentStringsW()) { }
		~EnvBlockWideScoper()  { ::FreeEnvironmentStringsW(m_envStringsW); m_envStringsW = NULL; } // not virtual
		bool IsOk()            { return (m_envStringsW != NULL); }
		LPWCH GetEnvStringsW() { return m_envStringsW; }
	};
};
