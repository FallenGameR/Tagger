#pragma once

namespace LeoHelpers
{
#ifndef _DEBUG
	inline void SetThreadName(LPCSTR szThreadName,  DWORD dwThreadId = -1) {}
	inline void SetThreadName(LPCWSTR szThreadName, DWORD dwThreadId = -1) {}
#else
	// SetThreadName in Visual Studio Debugger.
	// See http://msdn2.microsoft.com/en-us/library/xcb2z8hs.aspx

	#pragma pack(push,8)
	typedef struct tagTHREADNAME_INFO
	{
		DWORD dwType; // Must be 0x1000.
		LPCSTR szName; // Pointer to name (in user addr space).
		DWORD dwThreadID; // Thread ID (-1=caller thread).
		DWORD dwFlags; // Reserved for future use, must be zero.
	} THREADNAME_INFO;
	#pragma pack(pop)

	inline void SetThreadName(LPCSTR szThreadName, DWORD dwThreadId = -1)
	{
		Sleep(10);
		THREADNAME_INFO info;
		info.dwType = 0x1000;
		info.szName = szThreadName;
		info.dwThreadID = dwThreadId;
		info.dwFlags = 0;

		__try
		{
			::RaiseException(0x406D1388, 0, sizeof(info)/sizeof(ULONG_PTR), reinterpret_cast<ULONG_PTR*>(&info));
		}
		__except(EXCEPTION_CONTINUE_EXECUTION)
		{
		}
	}

	inline void SetThreadName(LPCWSTR szThreadName, DWORD dwThreadId = -1)
	{
		char *szAsciiName = LeoHelpers::WCtoMB(szThreadName);

		if (szAsciiName != NULL)
		{
			SetThreadName(szAsciiName);

			delete[] szAsciiName;
		}
	}
#endif

	class EventWrapper
	{
	private:
		HANDLE m_hEvent;
		EventWrapper(const EventWrapper &rhs); // disallow
		EventWrapper &operator=(const EventWrapper &rhs); // disallow
	public:
		EventWrapper(bool bManualReset, bool bInitialState)
		: m_hEvent(::CreateEvent(NULL, bManualReset?TRUE:FALSE, bInitialState?TRUE:FALSE, NULL))
		{
		}
		EventWrapper(LPSECURITY_ATTRIBUTES lpEventAttributes, bool bManualReset, bool bInitialState, LPCWSTR lpName)
		: m_hEvent(::CreateEvent(lpEventAttributes, bManualReset?TRUE:FALSE, bInitialState?TRUE:FALSE, lpName))
		{
		}
		~EventWrapper() // not virtual
		{
			if (m_hEvent != NULL)
			{
				::CloseHandle(m_hEvent);
				m_hEvent = NULL;
			}
		}
		inline bool IsOkay()                      { assert(m_hEvent != NULL); return (m_hEvent != NULL); }
		inline HANDLE GetEvent()                  { IsOkay(); return m_hEvent; }
		inline bool SetEvent()                    { return (IsOkay() && ::SetEvent(m_hEvent)); }
		inline bool ResetEvent()                  { return (IsOkay() && ::ResetEvent(m_hEvent)); }
		inline bool WaitForEvent(DWORD dwTimeout) { return (IsOkay() && ::WaitForSingleObject(m_hEvent, dwTimeout) == WAIT_OBJECT_0); }
		inline bool IsEventSet()                  { return WaitForEvent(0); }
	};

#ifdef _DEBUG
	class SingleThreadAsserter
	{
	private:
		DWORD m_dwThreadId;
	public:
		SingleThreadAsserter() : m_dwThreadId(::GetCurrentThreadId()) { }
		~SingleThreadAsserter() { } // NOT virtual
		SingleThreadAsserter(const SingleThreadAsserter &rhs)            { rhs.Assert(); m_dwThreadId = rhs.m_dwThreadId; }
		SingleThreadAsserter &operator=(const SingleThreadAsserter &rhs) { rhs.Assert(); m_dwThreadId = rhs.m_dwThreadId; return *this; }

		void Assert() const { assert(m_dwThreadId == 0 || m_dwThreadId == ::GetCurrentThreadId()); }
		void Neuter() { m_dwThreadId = 0; } // Call this to disable the asserts. (Zero is an invalid thread ID and may be used as a sentinal value.)
	};
#endif

	/*
	// A simple, use-once thread.
	class SimpleThread
	{
	public:
		SimpleThread();
		virtual ~SimpleThread();

		// Start the thread. Unlike HousekeepingThread::Start, it is *not* okay for two threads to call this at once.
		bool Start(); // Returns true if the thread is running.

		// Stop should never be called by two threads at once and in most cases should only ever be called implicitly by the destructor.
		void Stop();

	protected:
		virtual void MainTask() = 0; // Derived classes should override this to define the thread.

		HANDLE m_hStopEvent; // Derived classes should only use this to check if they've been asked to stop. Don't modify the handle.

#ifdef _DEBUG
		SingleThreadAsserter m_ownerThreadAssert;
#endif

	private:
		static unsigned __stdcall SimpleThread::staticThread(void *pVoidThis);

		SimpleThread(const SimpleThread &rhs); // disallow
		SimpleThread &operator=(const SimpleThread &rhs); // disallow

		HANDLE m_hThread;
	};
	*/


	// When Windows Vista is our support baseline, consider switching to the newer thread-pool API introduced in Vista.
	// (The thread-pool API in Win2k is pretty much worthless as it provides no clean way for the callback to stop or
	// re-schedule itself. Sigh.)
	class HousekeepingThread
	{
	public:
		HousekeepingThread(DWORD dwThreadIntervalMS);
		virtual ~HousekeepingThread();

		void SetThreadIntervalMS(DWORD dwThreadIntervalMS);
		DWORD GetThreadIntervalMS();

		// Start the thread if it is stopping or stopped. Unlike SimpleThread::Start, it *is* okay for two threads to call this at once.
		void Start();

		// Stop should never be called by two threads at once and in most cases should only ever be called implicitly by the destructor.
		void Stop();

	protected:
		// RequestStop should only be called by MainTask to prevent further iterations.
		void RequestStop();

		// Once the thread has started, MainTask is called every m_dwThreadIntervalMS until Stop or RequestStop has been called.
		virtual void MainTask() = 0;

	private:
		static unsigned __stdcall staticThread(void *pVoidThis);
		HousekeepingThread(const HousekeepingThread &rhs); // disallow
		HousekeepingThread &operator=(const HousekeepingThread &rhs); // disallow
	protected:
	//	CRITICAL_SECTION m_csThreadVariables;
	private:
		CRITICAL_SECTION m_csStart;
		DWORD m_dwThreadIntervalMS;
		HANDLE m_hThread;
	protected:
		HANDLE m_hStopEvent; // Derived classes should only use this to check if they've been asked to stop. Don't modify the handle.
	};

#if 0
	class LowIntegrity
	{
	public:
		LowIntegrity(HANDLE hThread);
		/* not virtual */ ~LowIntegrity();

		bool Succeeded() { return m_bSuccess; }

	private:
		LowIntegrity(const LowIntegrity &rhs); // disallow
		LowIntegrity &operator=(const LowIntegrity &rhs); // disallow

	private:
		bool m_bSuccess;
		HANDLE m_hThread;
	};
#endif

	class WorkerThread
	{
	public:
		class RunLockScoper
		{
		private:
			WorkerThread *m_pThread;
		public:
			RunLockScoper(WorkerThread *pThread) : m_pThread(pThread) { if (pThread != NULL) { pThread->LockRunning(); } }
			~RunLockScoper() { if (m_pThread != NULL) { m_pThread->UnlockRunning(); } } // non-virtual
		private:
			RunLockScoper(const RunLockScoper &rhs); // disallow
			RunLockScoper &operator=(const RunLockScoper &rhs); // disallow
		};

		class Job
		{
		private:
			volatile LONG m_lRefCount;
			LeoHelpers::ReleaseScoper< LeoHelpers::InstanceIdent > m_scJobId;

		public:
			Job() : m_lRefCount(1), m_scJobId(false, new LeoHelpers::InstanceIdent) {}

			LONG AddRef()  { return ::InterlockedIncrement(&m_lRefCount); };
			LONG Release() { LONG lRefCount = ::InterlockedDecrement(&m_lRefCount); if (lRefCount == 0) { delete this; } return lRefCount; };

			LeoHelpers::InstanceIdent *GetJobIdNoAddRef() { return m_scJobId.Get(); } // If the job itself will remain valid during use then you don't need to addref it; otherwise, you do.

			// A references is passed so you can modify the pointer (e.g. allocate and free it in the start/end jobs).
			virtual void ExecuteJob(void *&pvUserData, WorkerThread *pWorkerThread) = 0;
			
			// CancelJob is called on queued jobs if the thread is shut-down before they run.
			// CancelJob is not called on the start/end jobs, even if the jobs are never run because the thread is never started before the thread object is destroyed.
			// CancelJob must not assume that the start and/or end jobs have or have not run. CancelJob can be called before or after them, when they've never been called at all, or even while they are being called.
			// You'd typically use CancelJob to set an event which the requester was waiting on to let it know to stop waiting.
			virtual void CancelJob(void *&pvUserData, WorkerThread *pWorkerThread) = 0; // { }

		protected:
			virtual ~Job() { } // Deletion must be through calling Release, not directly, so this is protected.

		private:
			Job(const Job &rhs); // disallow
			Job &operator=(const Job &rhs); // disallow
		};

	private:
		struct JobAndPriority
		{
			Job * m_pJob;
			int   m_pri;

			JobAndPriority(Job *pJob, int pri) : m_pJob(pJob), m_pri(pri) { }
		};

		struct JobAndPriorityGT
		{
			inline bool operator()(const JobAndPriority &lhs, const JobAndPriority &rhs) const
			{
				return lhs.m_pri > rhs.m_pri;
			}
		};

		mutable CRITICAL_SECTION     m_cs;
		volatile LONG                m_lLockRunningCount;
		HANDLE                       m_hThread;    // Within m_cs, if non-NULL then the thread is valid and can have events queued to it.
		HANDLE                       m_hThreadOld; // Threads that are about to terminate move their handles to here (waiting for any previous thread to finish if required).
		HANDLE                       m_hEventWake;
		HANDLE                       m_hEventThreadMayBeStopped;
		HANDLE                       m_hEventThreadMayBeIdle; // Set when no jobs are in the queue or executing.
		bool                         m_bStop;
		int                          m_iStopAsSoonAsIdle;
		DWORD                        m_dwIdleTimeoutMS;
		std::deque< JobAndPriority > m_requests; // Do not use a priority_queue or multimap as neither would guarantee to preserve the added-date order of elements with the same priority.
		size_t                       m_pendingRequestCount; // not the same as m_requests.size() since it includes the running job (if any).
		void *                       m_pvUserData;
		Job *                        m_pJobThreadStart; // Run when the worker thread (re-)starts.
		Job *                        m_pJobThreadEnd;   // Run when the worker thread stops.
		Job *                        m_pJobPreJob;      // Run before each queued job.
		Job *                        m_pJobPostJob;     // Run after each queued job.
		Job *                        m_pJobIdle;        // Run after one or more queued jobs, when the queue becomes empty, unless the thread immediately stops or restarts.
		Job *                        m_pJobPreWait;     // Run before the worker thread waits. Very similar to, and run just after, the idle-job; difference is it is always run before waiting even if no queued jobs just ran, and should not assume the start/end jobs have or haven't run.
		
		LeoHelpers::ReleaseScoper< LeoHelpers::InstanceIdent > m_scCurrentJobId; // ID of job which was queued and is running (or just finished, pending WorkerThread tidy-up) on the thread right now. (Never any of the jobs above; they don't get queued.)
		std::set< HANDLE >           m_setJobChangeWaitHandles; // Handles waiting on the current job (i.e. m_scCurrentJobId) to change.

	public:

		// pvUserData is optional and is passed to all Job::ExecuteJob calls.
		// pJobThreadStart and pJobThreadEnd are optional and will be run when the worker thread (re-)starts and finishes.
		// pJobPreJob and pJobPostJob are also optional and will be run before and after each queued job.
		// pJobIdle is also optional and will be run after one or more queued jobs, when the queue becomes empty, unless the thread is shutting down or restarting.
		// pJobPreWait is also optional and will be run before each time the thread waits. Similar to the idle job but: a) It is always run before waiting (even if no queued jobs just ran); b) It must not assume that the start/end jobs have or haven't been run.
		// Give dwIdleTimeoutMS as INFINITE if you don't want the thread to start and stop automatically (it'll still only start after the first Queue call).
		WorkerThread(DWORD dwIdleTimeoutMS, void *pvUserData, Job *pJobThreadStart, Job *pJobThreadEnd, Job *pJobPreJob, Job *pJobPostJob, Job *pJobIdle, Job *pJobPreWait);
		virtual ~WorkerThread();

		bool StartRunning();          // Manually starts the thread even if its queue is empty. Does not lock it running; you should call LockRunning *before* StartRunning if you need it locked, otherwise it may stop after the idle time.
		bool IsLockedRunning();       // Returns true if LockRunning has been called more times than UnlockRunning. Does not actually lock or run anything.
		LONG LockRunning();           // Locks the thread running if or when it is started.
		LONG UnlockRunning();         // Every call to LockRunning must be balanced by a call to UnlockRunning.

		DWORD GetIdleTimeout() const;
		void SetIdleTimeout(DWORD dwIdleTimeoutMS);

		// Requests the thread to shut down if it isn't doing anything for anyone.
		// Returns false if the stop request was not sent or if there was a timeout waiting for the thread to stop.
		// Never assume the thread is actually stopped after this returns, even if it returns true.
		// The thread may have started up again, or not finished stopping, after the call.
		// If you need to know the thread has actually stopped, delete the WorkerThread object.
		bool StopIfIdle(DWORD dwWaitForStop);

		bool Queue(Job *pJob, int priority); // Returns boolean success. Starts the thread if required.
		bool QueueIfIdle(Job *pJob, int priority, bool *pbFailedBecauseNotIdle); // Returns boolean success. Starts the thread if required. Fails if the thread is not idle (if there are pending jobs or it is locked-running).
		bool IsIdle() const; // Returns true if the thread is idle (no pending jobs and not locked-running). Of course, unless you ensure otherwise, something could queue a job right after the check.
		void FlushQueue(bool bAssertIfLengthy); // Waits until there are no pending or executing jobs. Of course, unless you ensure otherwise, something could queue a job right after the check.

		bool CancelJobIfQueued(const LeoHelpers::InstanceIdent *pJobId); // Returns true iff the job was in the queue and got cancelled.
		DWORD WaitForJobIfRunning(const LeoHelpers::InstanceIdent *pJobId, DWORD dwTimeout); // Returns WAIT_OBJECT_0 if job was running and finished in time, WAIT_TIMEOUT if job running and didn't finish in time, WAIT_FAILED if job wasn't running (note that it may have been queued; call CancelJobIfQueued first to avoid that).

		Job *GetStartJob() { return m_pJobThreadStart; }
		Job *GetEndJob()   { return m_pJobThreadEnd; }

	private:
		inline bool initOkay() const { return (m_hEventWake != NULL && m_hEventThreadMayBeStopped != NULL && m_hEventThreadMayBeIdle != NULL); }

		bool isIdleAlreadyInCS() const;
		void cleanUpOldThreadAlreadyInCS(); 
		bool startThreadIfNeededAlreadyInCS(bool *pbAlreadyRunning);
		bool queueInternal(Job *pJob, int priority, bool bFailIfNotIdle, bool *pbFailedBecauseNotIdle);
		void purgeQueueAlreadyInCS();

		static unsigned __stdcall staticThread(void *pVoidThis);
		void thread();

	private:
		WorkerThread(const WorkerThread &rhs); // disallow
		WorkerThread &operator=(const WorkerThread &rhs); // disallow
	};

//	bool WaitAndProcessSent(const int cHandles, HANDLE *pWaitHandles);

	bool CreateProcessWrapper(const wchar_t *szExePath, const wchar_t *szArgsWithoutExe, const wchar_t *szCurDir, bool bExeDirAsCurDir, HANDLE *phProcess);
};
