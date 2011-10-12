#include "stdafx.h"
#include "LeoHelpers_String.h"
#include "LeoHelpers_File.h"
#include "LeoHelpers_Scope.h"
#include "LeoHelpers_Thread.h"
#include "LeoHelpers_Debug.h"

#ifdef _DEBUG
//#define LEOHELPERS_HOUSEKEEPER_DEBUG
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
LeoHelpers::SimpleThread::SimpleThread()
: m_hThread(NULL)
, m_hStopEvent( ::CreateEvent(NULL, TRUE, FALSE, NULL) )
{
}

LeoHelpers::SimpleThread::~SimpleThread()
{
	Stop();

	if (NULL != m_hStopEvent)
	{
		CloseHandle(m_hStopEvent);
		m_hStopEvent = NULL;
	}
}

bool LeoHelpers::SimpleThread::Start()
{
#ifdef _DEBUG
	m_ownerThreadAssert.Assert();
#endif

	if (NULL != m_hThread
	||	NULL == m_hStopEvent)
	{
		return false;
	}

	unsigned int uiIgnored;

	m_hThread = reinterpret_cast<HANDLE>( ::_beginthreadex(NULL, 0, staticThread, this, 0, &uiIgnored) );

	return (m_hThread != NULL);
}

void LeoHelpers::SimpleThread::Stop()
{
#ifdef _DEBUG
	m_ownerThreadAssert.Assert();
#endif

	if (NULL != m_hThread)
	{
		if (NULL != m_hStopEvent)
		{
			::SetEvent(m_hStopEvent);
			::WaitForSingleObject(m_hThread, INFINITE);
			::ResetEvent(m_hStopEvent);
		}
		::CloseHandle(m_hThread);
		m_hThread = NULL; // This must be the only way for m_hThread to become null again.
	}
}

unsigned __stdcall LeoHelpers::SimpleThread::staticThread(void *pVoidThis)
{
	LeoHelpers::SetThreadName("Plugin simple thread");

	SimpleThread *pThis = reinterpret_cast<SimpleThread *>(pVoidThis);

	pThis->MainTask();

	return 0;
}
*/

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

LeoHelpers::HousekeepingThread::HousekeepingThread(DWORD dwThreadIntervalMS)
: m_hThread(NULL)
, m_hStopEvent( ::CreateEvent(NULL, TRUE, FALSE, NULL) )
, m_dwThreadIntervalMS(dwThreadIntervalMS)
{
	InitializeCriticalSection(&m_csStart);
//	InitializeCriticalSection(&m_csThreadVariables);
}

LeoHelpers::HousekeepingThread::~HousekeepingThread()
{
	Stop();
	if (NULL != m_hStopEvent)
	{
		CloseHandle(m_hStopEvent);
		m_hStopEvent = NULL;
	}

	DeleteCriticalSection(&m_csStart);
//	DeleteCriticalSection(&m_csThreadVariables);
}

void LeoHelpers::HousekeepingThread::SetThreadIntervalMS(DWORD dwThreadIntervalMS)
{
	// DWORD access is atomic so we don't need m_csThreadVariables.
//	LeoHelpers::CriticalSectionScoper css(&m_csThreadVariables);
	m_dwThreadIntervalMS = dwThreadIntervalMS;
}

DWORD LeoHelpers::HousekeepingThread::GetThreadIntervalMS()
{
	// DWORD access is atomic so we don't need m_csThreadVariables.
//	LeoHelpers::CriticalSectionScoper css(&m_csThreadVariables);
	return m_dwThreadIntervalMS;
}

// Start the thread if it is stopping or stopped.
void LeoHelpers::HousekeepingThread::Start()
{
	// Critical section deals with case where two threads are calling Start at once.
	LeoHelpers::CriticalSectionScoper css(&m_csStart);

#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
	OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"Start", L"Start requested");
#endif

	if (NULL != m_hThread && WAIT_OBJECT_0 == WaitForSingleObject(m_hStopEvent, 0))
	{
		// The thread is stopping. Wait for it to finish.
		Stop();
	}

	if (NULL == m_hThread)
	{
		// The thread is not running. Start a new one.

		unsigned int uiIgnored;

		m_hThread = reinterpret_cast<HANDLE>(_beginthreadex(NULL, 0, staticThread, this, 0, &uiIgnored));
	}
}

// Stop should never be called by two threads at once and in most cases should only ever be called implicitly by the destructor.
void LeoHelpers::HousekeepingThread::Stop()
{
#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
	OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"Stop", L"Stop requested");
#endif

	if (NULL != m_hThread)
	{
		if (NULL != m_hStopEvent)
		{
			SetEvent(m_hStopEvent);
			WaitForSingleObject(m_hThread, INFINITE);
			ResetEvent(m_hStopEvent);
		}
		CloseHandle(m_hThread);
		m_hThread = NULL; // This must be the only way for m_hThread to become null again.
	}
}

// RequestStop should only be called by MainTask to prevent further iterations.
void LeoHelpers::HousekeepingThread::RequestStop()
{
#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
	OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"RequestStop", L"Stop requested");
#endif

	if (NULL != m_hStopEvent)
	{
		SetEvent(m_hStopEvent);
	}
}

unsigned __stdcall LeoHelpers::HousekeepingThread::staticThread(void *pVoidThis)
{
	LeoHelpers::SetThreadName("Plugin Housekeeping");

#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
	OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"staticThread", L"Start");
#endif

	HousekeepingThread *pThis = reinterpret_cast<HousekeepingThread *>(pVoidThis);

	if (NULL == pThis->m_hStopEvent) { return 0; }

	while(WAIT_TIMEOUT == WaitForSingleObject(pThis->m_hStopEvent, pThis->GetThreadIntervalMS()))
	{
#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
		OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"staticThread", L"Wake");
#endif

		pThis->MainTask();

#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
		OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"staticThread", L"Sleep");
#endif
	}

#ifdef LEOHELPERS_HOUSEKEEPER_DEBUG
		OutputDebugFormat(L"[LeoHelpers] ", L"HousekeepingThread", L"staticThread", L"End");
#endif

	return 0;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if 0
LeoHelpers::LowIntegrity::LowIntegrity(HANDLE hThread)
: m_hThread(hThread)
, m_bSuccess(false)
{
	if (m_hThread == NULL)
	{
		m_hThread = ::GetCurrentThread(); // Does not need closing.
	}

	if (::ImpersonateSelf(SecurityImpersonation))
	{
		HANDLE hThreadToken;

		if (::OpenThreadToken(m_hThread, MAXIMUM_ALLOWED, TRUE, &hThreadToken))
		{
			LeoHelpers::CHandleScoper hsThreadToken(hThreadToken);
			hThreadToken = INVALID_HANDLE_VALUE;

			// Low integrity SID
			const wchar_t *szIntegritySid = L"S-1-16-4096"; // 16 = SECURITY_MANDATORY_LABEL_AUTHORITY, 4096 = SECURITY_MANDATORY_LOW_RID
			PSID pIntegritySid = NULL;

			if (::ConvertStringSidToSid(szIntegritySid, &pIntegritySid))
			{
				LeoHelpers::LocalFreeScoper lfsSid(pIntegritySid);

				TOKEN_MANDATORY_LABEL til = {0};
				til.Label.Attributes = SE_GROUP_INTEGRITY | SE_GROUP_INTEGRITY_ENABLED;
				til.Label.Sid        = pIntegritySid;

				if (::SetTokenInformation(hsThreadToken.Get(), TokenIntegrityLevel, &til, sizeof(til) + GetLengthSid(pIntegritySid)))
				{
					if (::SetThreadToken(&m_hThread, hsThreadToken.Get()))
					{
						m_bSuccess = true;
					}
				}
			}

			if (!m_bSuccess)
			{
				::RevertToSelf();
			}
		}
	}
}

LeoHelpers::LowIntegrity::~LowIntegrity()
{
	if (m_bSuccess)
	{
		::SetThreadToken(&m_hThread, NULL);
		::RevertToSelf();
		m_bSuccess = false;
	}
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

LeoHelpers::WorkerThread::WorkerThread(DWORD dwIdleTimeoutMS, void *pvUserData, Job *pJobThreadStart, Job *pJobThreadEnd, Job *pJobPreJob, Job *pJobPostJob, Job *pJobIdle, Job *pJobPreWait)
: m_lLockRunningCount(0)
, m_hThread(NULL)
, m_hThreadOld(NULL)
, m_hEventWake(::CreateEvent(NULL, TRUE, TRUE, NULL))
, m_hEventThreadMayBeStopped(::CreateEvent(NULL, TRUE, TRUE, NULL))
, m_hEventThreadMayBeIdle(::CreateEvent(NULL, TRUE, TRUE, NULL))
, m_bStop(false)
, m_iStopAsSoonAsIdle(0)
, m_dwIdleTimeoutMS(dwIdleTimeoutMS)
, m_pvUserData(pvUserData)
, m_pJobThreadStart(pJobThreadStart)
, m_pJobThreadEnd(pJobThreadEnd)
, m_pJobPreJob(pJobPreJob)
, m_pJobPostJob(pJobPostJob)
, m_pJobIdle(pJobIdle)
, m_pJobPreWait(pJobPreWait)
, m_pendingRequestCount(0)
{
	InitializeCriticalSection(&m_cs);

	if (m_pJobThreadStart != NULL) { m_pJobThreadStart->AddRef(); }
	if (m_pJobThreadEnd   != NULL) { m_pJobThreadEnd->AddRef();   }
	if (m_pJobPreJob      != NULL) { m_pJobPreJob->AddRef();      }
	if (m_pJobPostJob     != NULL) { m_pJobPostJob->AddRef();     }
	if (m_pJobIdle        != NULL) { m_pJobIdle->AddRef();        }
	if (m_pJobPreWait     != NULL) { m_pJobPreWait->AddRef();     }
}

LeoHelpers::WorkerThread::~WorkerThread()
{
	leoAssert(m_lLockRunningCount == 0);

	if (initOkay())
	{
		HANDLE hThreadCurrent = NULL;

		{
			LeoHelpers::CriticalSectionScoper css(&m_cs);

			leoAssert(m_hThread == NULL || m_hThreadOld == NULL); // This checks that code that starts a new thread waited for the old one (if any) to finish. They could both be NULL but shouldn't both be non-NULL.

			cleanUpOldThreadAlreadyInCS(); // Wait for and clean up the old thread handle (if any).

			m_bStop = true;
			hThreadCurrent = m_hThread; // Remember the current thread (if any) as the handle will be moved into m_hOldThread by the thread itself.
		}

		::SetEvent(m_hEventWake); // Some time after this the current thread (if any) will become the new-old thread as it shuts down.

		if (hThreadCurrent != NULL)
		{
			// No need to re-enter m_cs here as there can't be any other threads accessing us in the destructor when our worker thread has stopped.

			DWORD dwWaitRes = ::WaitForSingleObject(hThreadCurrent, INFINITE); // Wait for the current thread to finish.

			leoAssert(dwWaitRes == WAIT_OBJECT_0);
			leoAssert(m_hThread == NULL);
			leoAssert(m_hThreadOld == hThreadCurrent);

			::CloseHandle(hThreadCurrent);
			hThreadCurrent = NULL;
			m_hThreadOld = NULL;
		}
	}

	leoAssert(m_requests.empty());
	leoAssert(m_pendingRequestCount == 0);
	leoAssert(m_hThread == NULL);
	leoAssert(m_hThreadOld == NULL);

	if (m_hEventWake              ) { ::CloseHandle(m_hEventWake);               m_hEventWake               = NULL; }
	if (m_hEventThreadMayBeStopped) { ::CloseHandle(m_hEventThreadMayBeStopped); m_hEventThreadMayBeStopped = NULL; }
	if (m_hEventThreadMayBeIdle   ) { ::CloseHandle(m_hEventThreadMayBeIdle);    m_hEventThreadMayBeIdle    = NULL; }

	m_pvUserData = NULL;

	if (m_pJobThreadStart != NULL) { m_pJobThreadStart->Release(); m_pJobThreadStart = NULL; }
	if (m_pJobThreadEnd   != NULL) { m_pJobThreadEnd->Release();   m_pJobThreadEnd   = NULL; }
	if (m_pJobPreJob      != NULL) { m_pJobPreJob->Release();      m_pJobPreJob      = NULL; }
	if (m_pJobPostJob     != NULL) { m_pJobPostJob->Release();     m_pJobPostJob     = NULL; }
	if (m_pJobIdle        != NULL) { m_pJobIdle->Release();        m_pJobIdle        = NULL; }
	if (m_pJobPreWait     != NULL) { m_pJobPreWait->Release();     m_pJobPreWait     = NULL; }

	DeleteCriticalSection(&m_cs);
}

void LeoHelpers::WorkerThread::cleanUpOldThreadAlreadyInCS()
{
	if (m_hThreadOld)
	{
		// Wait for the old thread to finish and clean-up its thread handle.
		// It's safe to do this wait within the critical section as the worker thread must only move its handle
		// to the old slot after it's done using the critical section and is about to terminate.
		::WaitForSingleObject(m_hThreadOld, INFINITE);
		::CloseHandle(m_hThreadOld);
		m_hThreadOld = NULL;
	}
}

bool LeoHelpers::WorkerThread::startThreadIfNeededAlreadyInCS(bool *pbAlreadyRunning)
{
	cleanUpOldThreadAlreadyInCS();

	if (m_hThread != NULL)
	{
		if (pbAlreadyRunning != NULL)
		{
			*pbAlreadyRunning = true;
		}

		return true;
	}

	if (pbAlreadyRunning != NULL)
	{
		*pbAlreadyRunning = false;
	}

	unsigned int uiIgnored = 0;

	m_hThread = reinterpret_cast< HANDLE >( ::_beginthreadex(NULL, 0, staticThread, this, 0, &uiIgnored) );

	if (m_hThread != NULL)
	{
		// Note: The new thread starts awake; no need to wake it.
		return true;
	}

	return false;
}

bool LeoHelpers::WorkerThread::StartRunning()
{
	bool bResult = false;

	leoAssert(initOkay());

	if (initOkay())
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		bResult = startThreadIfNeededAlreadyInCS(NULL);
	}

	return bResult;
}


bool LeoHelpers::WorkerThread::isIdleAlreadyInCS() const
{
	return (m_lLockRunningCount == 0 && m_pendingRequestCount == 0);
}

// Requests the thread to shut down if it isn't doing anything for anyone.
// Returns false if the stop request was not sent or if there was a timeout waiting for the thread to stop.
// Never assume the thread is actually stopped after this returns, even if it returns true.
// The thread may have started up again, or not finished stopping, after the call.
// If you need to know the thread has actually stopped, delete the WorkerThread object.
bool LeoHelpers::WorkerThread::StopIfIdle(DWORD dwWaitForStop)
{
	bool bResult = false;

	leoAssert(initOkay());

	if (initOkay())
	{
		bool bRequestStop = false;

		{
			LeoHelpers::CriticalSectionScoper css(&m_cs);

			if (isIdleAlreadyInCS())
			{
				++m_iStopAsSoonAsIdle;
				bRequestStop = true;
			}
		}

		if (bRequestStop)
		{
			::SetEvent(m_hEventWake);

			// This event is unreliable. The thread may (re-)start even if it is fired, and it may never be fired if the thread
			// becomes busy with work just after we exit the above critical section.
			if (::WaitForSingleObject(m_hEventThreadMayBeStopped, dwWaitForStop) == WAIT_OBJECT_0)
			{
				bResult = true;
			}

			{
				LeoHelpers::CriticalSectionScoper css(&m_cs);
				--m_iStopAsSoonAsIdle;
			}
		}
	}

	return bResult;
}

void LeoHelpers::WorkerThread::FlushQueue(bool bAssertIfLengthy)
{
	leoAssert(initOkay());

#ifdef _DEBUG
	if (bAssertIfLengthy)
	{
		DWORD dwWaitRes = ::WaitForSingleObject(m_hEventThreadMayBeIdle, 500);

		leoAssert(dwWaitRes != WAIT_TIMEOUT); // If this fires, the caller expected the thread was about to be destroyed but it's probably actually running something.

		if (dwWaitRes != WAIT_TIMEOUT)
		{
			return;
		}
	}
#endif

	::WaitForSingleObject(m_hEventThreadMayBeIdle, INFINITE);
}

bool LeoHelpers::WorkerThread::CancelJobIfQueued(const LeoHelpers::InstanceIdent *pJobId)
{
	leoAssert(pJobId != NULL);
	if (pJobId == NULL)
	{
		return false;
	}

	bool bFound = false;

	LeoHelpers::CriticalSectionScoper css(&m_cs);

	for (std::deque< JobAndPriority >::iterator diter = m_requests.begin(); diter != m_requests.end(); ++diter)
	{
		if (diter->m_pJob->GetJobIdNoAddRef()->IsEqualTo(pJobId))
		{
			diter->m_pJob->CancelJob(m_pvUserData, this);
			diter->m_pJob->Release();

			m_requests.erase(diter);

			leoAssert(m_pendingRequestCount != 0);
			--m_pendingRequestCount;

			bFound = true;
			break;
		}
	}

	if (bFound && m_pendingRequestCount==0)
	{
		::SetEvent(m_hEventThreadMayBeIdle);
	}

	return bFound;
}

DWORD LeoHelpers::WorkerThread::WaitForJobIfRunning(const LeoHelpers::InstanceIdent *pJobId, DWORD dwTimeout)
{
	// Returns WAIT_OBJECT_0 if job was running and finished in time.
	//         WAIT_TIMEOUT  if job was running and didn't finish in time (can also happen if same job instance in queue twice or immediately re-run).
	//         WAIT_FAILED   if job wasn't running (note that it may have been queued; call CancelJobIfQueued first to avoid that).

	leoAssert(pJobId != NULL);
	if (pJobId == NULL)
	{
		return WAIT_FAILED;
	}

	// See if the job is running in case it isn't and we can return without creating the event etc.

	// scope CS
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		if (m_scCurrentJobId.IsEmpty() || m_scCurrentJobId.Get()->IsDifferentTo(pJobId))
		{
			return WAIT_FAILED; // Specified job was not running.
		}
	}

	LeoHelpers::EventWrapper ewJobChanged(true, false);

	// scope CS
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		if (m_scCurrentJobId.IsEmpty() || m_scCurrentJobId.Get()->IsDifferentTo(pJobId))
		{
			return WAIT_OBJECT_0; // Specified job was running and has now stopped.
		}

		m_setJobChangeWaitHandles.insert(ewJobChanged.GetEvent());
	}

	DWORD dwWaitRes = ewJobChanged.WaitForEvent(dwTimeout);

	leoAssert(dwWaitRes == WAIT_OBJECT_0 || dwWaitRes == WAIT_TIMEOUT);

	// scope CS
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		m_setJobChangeWaitHandles.erase(ewJobChanged.GetEvent());

		if (m_scCurrentJobId.IsEmpty() || m_scCurrentJobId.Get()->IsDifferentTo(pJobId))
		{
			return WAIT_OBJECT_0; // Specified job was running and has now stopped.
		}
	}

	leoAssert(dwWaitRes != WAIT_OBJECT_0); // If it is WAIT_OBJECT_0 then the job changed but was still the same job when we checked. Presumably the same job instance has been queued twice or was immediately re-used, both of which should be avoided (create a new job instance instead).

	return WAIT_TIMEOUT; // Specified job was still running after the wait/timeout
}

bool LeoHelpers::WorkerThread::IsIdle() const
{
	leoAssert(initOkay());
	
	LeoHelpers::CriticalSectionScoper css(&m_cs);

	// Of course, this may change the moment we return; up to the caller to secure exclusive access to the object.
	// We still use m_cs, though. It'd be valid for a caller to check IsIdle, then secure exclusive access, then double-check IsIdle.
	return isIdleAlreadyInCS();
}

bool LeoHelpers::WorkerThread::queueInternal(Job *pJob, int priority, bool bFailIfNotIdle, bool *pbFailedBecauseNotIdle)
{
	leoAssert(pJob != NULL);
	leoAssert(initOkay());

	bool bWake = false;
	bool bResult = false;
	if (pbFailedBecauseNotIdle)
	{
		*pbFailedBecauseNotIdle = false;
	}

	if (initOkay())
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		if (bFailIfNotIdle && !isIdleAlreadyInCS())
		{
			if (pbFailedBecauseNotIdle)
			{
				*pbFailedBecauseNotIdle = true;
			}
		}
		else
		{
			bResult = startThreadIfNeededAlreadyInCS(&bWake);

			if (bResult)
			{
				// If there's a new thread (or woken old thread) then it'll be waiting on m_cs before it can
				// inspect m_requests. We're still in m_cs so it's fine to update m_requests now.

				// We insert the new job into the list before the first item that has a lower priority than it.
				// upper_bound is often described as "finding the first item that is greater than the given item"
				// but we supply an inverted "<" predicate, which does ">" instead, to find the first lesser item.)

				// By doing this we keep the queue sorted by priority, with each sequence of items of the same priority
				// kept sorted in FIFO order. (Maintaining FIFO for each priority is why we don't use a priority_queue or multiset.)

				JobAndPriority jap( pJob, priority );
				m_requests.insert( std::upper_bound(m_requests.begin(), m_requests.end(), jap, JobAndPriorityGT()), jap );
				pJob->AddRef();
				++m_pendingRequestCount;
				::ResetEvent(m_hEventThreadMayBeIdle);
			}
		}
	}

	if (bWake)
	{
		::SetEvent(m_hEventWake); // Done outside of m_cs so that the thread isn't blocked by us if it wakes immediately.
	}

	return bResult;
}

bool LeoHelpers::WorkerThread::Queue(Job *pJob, int priority)
{
	return queueInternal(pJob, priority, false, NULL);
}

bool LeoHelpers::WorkerThread::QueueIfIdle(Job *pJob, int priority, bool *pbFailedBecauseNotIdle)
{
	return queueInternal(pJob, priority, true, pbFailedBecauseNotIdle);
}

void LeoHelpers::WorkerThread::purgeQueueAlreadyInCS()
{
	leoAssert(m_requests.empty()); // It'll be abnormal for the thread to be shut-down when the queue is non-empty, so investigate if it happens.
	
	while(!m_requests.empty())
	{
		m_requests.front().m_pJob->CancelJob(m_pvUserData, this);
		m_requests.front().m_pJob->Release();
		m_requests.pop_front();
		leoAssert(m_pendingRequestCount != 0); // Unlike the first assert, this one firing means there really is a bug.
		--m_pendingRequestCount;
	}

	leoAssert(m_pendingRequestCount==0); // Unlike the first assert, this one firing means there really is a bug.

	::SetEvent(m_hEventThreadMayBeIdle);
}

unsigned __stdcall LeoHelpers::WorkerThread::staticThread(void *pVoidThis)
{
	LeoHelpers::SetThreadName("Plugin WorkerThread");

	reinterpret_cast< WorkerThread * >( pVoidThis )->thread();

	return 0;
}

void LeoHelpers::WorkerThread::thread()
{
	leoAssert(m_hThreadOld == NULL); // Old thread must have stopped running before we were started.

	// Ensure the first wait doesn't actually wait, so we setup everything on the first iteration.
	::SetEvent(m_hEventWake);
	bool bLock = false;
	bool bStopAsSoonAsIdle = false;
	bool bHaveRunStartJob = false;
	bool bNeedToRunIdleJob = false;
	bool bDecrementJobCount = false;
	DWORD dwIdleTimeoutMS = 0;

	while(true)
	{
		if (bNeedToRunIdleJob)
		{
			if (m_pJobIdle != NULL)
			{
				m_pJobIdle->ExecuteJob(m_pvUserData, this);
			}

			bNeedToRunIdleJob = false;
		}

		if (m_pJobPreWait != NULL)
		{
			m_pJobPreWait->ExecuteJob(m_pvUserData, this);
		}

		// m_cs scope
		{
			LeoHelpers::CriticalSectionScoper css(&m_cs);
			dwIdleTimeoutMS = m_dwIdleTimeoutMS;
		}

		DWORD dwWaitRes = ::WaitForSingleObject(m_hEventWake, bLock ? INFINITE : bStopAsSoonAsIdle ? 0 : dwIdleTimeoutMS);
		::ResetEvent(m_hEventWake);

		while(true)
		{
			Job *pJob = NULL;

			// m_cs scope
			{
				LeoHelpers::CriticalSectionScoper css(&m_cs);

				m_scCurrentJobId.ReleaseChild();
				for (std::set< HANDLE >::const_iterator handIter = m_setJobChangeWaitHandles.begin(); handIter != m_setJobChangeWaitHandles.end(); ++handIter)
				{
					::SetEvent(*handIter); // It may change again after this (if there's another job) but not until we leave the CS.
				}

				if (bDecrementJobCount)
				{
					bDecrementJobCount = false;
					leoAssert(m_pendingRequestCount != 0);
					if (--m_pendingRequestCount == 0)
					{
						::SetEvent(m_hEventThreadMayBeIdle);
					}
				}

				if ((m_bStop) // If you change any of this, remember to change the identical check below.
				||	(dwWaitRes != WAIT_TIMEOUT && dwWaitRes != WAIT_OBJECT_0)
				||	(dwWaitRes == WAIT_TIMEOUT && m_lLockRunningCount == 0 && m_requests.empty()))
				{
					purgeQueueAlreadyInCS();

					// Run the thread-finish job, outside of the critical section, if there is one and if we actually ran the thread-start job.
					if (bHaveRunStartJob && m_pJobThreadEnd != NULL)
					{
						LeoHelpers::CriticalSectionScoperReverse decss(&m_cs); // Duck out of the critical section to run the thread-end job.

						m_pJobThreadEnd->ExecuteJob(m_pvUserData, this);
					}

					// As the name suggests, the m_hEventThreadMayBeStopped event is not precise.
					// Even if the event is set the thread may not have actually stopped yet and may decide not to stop at all.
					// The event is just here so things which would like the thread to stop if it can, but don't absolutely need it to,
					// can synchronise themselves to some degree. We set the event here, rather than past the point of no return, on
					// purpose so that something waiting on it will not have to timeout. Anything waiting on it knows it is unreliable.
					::SetEvent(m_hEventThreadMayBeStopped);

					// Check that we're still shutting down
					if ((m_bStop) // If you change any of this, remember to change the identical check above.
					||	(dwWaitRes != WAIT_TIMEOUT && dwWaitRes != WAIT_OBJECT_0)
					||	(dwWaitRes == WAIT_TIMEOUT && m_lLockRunningCount == 0 && m_requests.empty()))
					{
						purgeQueueAlreadyInCS(); // Purge the queue again in case requests came in while we ran the thread-end job and we're still shutting down due to m_bStop or wait-error.

						// This is the point of no-return for this thread. It now must terminate.
						// Move our thread handle into m_hThreadOld so the rest of the program can still wait for us to fully terminate
						// (important during shut-down), but knows that a new thread needs to be started if more requests need queueing.
						leoAssert(m_hThreadOld == NULL); // Old thread must have stopped running before we were started.
						m_hThreadOld = m_hThread;
						m_hThread = NULL;
						return; // The thread is done.
					}
					else
					{
						// We were going to shut-down due to idle time-out but a request or run-lock must've come in while we were executing the ThreadEnd job.
						// We're going to recycle this thread and thus need to re-run the ThreadStart job as if this was a brand-new thread.
						bHaveRunStartJob = false;
						bNeedToRunIdleJob = false; // Idle job is cancelled by the Thread-End job, so don't run it again until a normal job has been run again.
					}
				}

				if (m_requests.empty())
				{
					// We cache the lock status here since we're already in the critical section.
					// It doesn't matter if the lock status is out-of-date after we exit the critical section
					// as changes between locked/unlocked set the wake event and will make us loop again if needed.
					bLock = (m_lLockRunningCount > 0);
					bStopAsSoonAsIdle = !bLock && (m_iStopAsSoonAsIdle > 0);
					break; // Exit the inner loop and go back to the main loop where we wait for more requests or the idle timeout.
				}

				dwWaitRes = WAIT_OBJECT_0; // Even if we woke due to the timeout, pretend we woke up due to the queue as there's stuff in there now.

				pJob = m_requests.front().m_pJob;
				m_scCurrentJobId.SetWithAddRef(pJob->GetJobIdNoAddRef());
				m_requests.pop_front();
				bDecrementJobCount = true; // Means m_pendingRequestCount will be decremented at the top of the next iteration.
			}
			// left m_cs

			// Run the thread-start job, if it hasn't been run already, before running any actual jobs.
			if (!bHaveRunStartJob)
			{
				// Thread definitely isn't stopped now. (Anything waiting on this event should know it is unreliable.)
				::ResetEvent(m_hEventThreadMayBeStopped);

				if (m_pJobThreadStart != NULL)
				{
					m_pJobThreadStart->ExecuteJob(m_pvUserData, this);
				}

				bHaveRunStartJob = true; // Flag is also used to decide whether or not to run the thread-end job so it must be maintained even if the start job is NULL.
			}

			if (m_pJobPreJob != NULL)
			{
				m_pJobPreJob->ExecuteJob(m_pvUserData, this);
			}

			// Run the actual job.
			pJob->ExecuteJob(m_pvUserData, this);
			pJob->Release();

			bNeedToRunIdleJob = true;

			if (m_pJobPostJob != NULL)
			{
				m_pJobPostJob->ExecuteJob(m_pvUserData, this);
			}

			// Continue the inner loop, decrememnting m_pendingRequestCount and then looking for more jobs of a wait/stop condition.
		}

		// Continue the outer loop, waiting.
	}

	// (The loop contains a return statement. Code here will not be run.)
}

bool LeoHelpers::WorkerThread::IsLockedRunning()
{
	leoAssert(initOkay());

	if (initOkay())
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		leoAssert(m_lLockRunningCount >= 0);

		if (m_lLockRunningCount > 0)
		{
			return true;
		}
	}

	return false;
}

// LockRunning does not start the thread if it isn't running; it only keeps it running if or when it has started running.
LONG LeoHelpers::WorkerThread::LockRunning()
{
	LONG lResult = -1000;

	leoAssert(initOkay());

	if (initOkay())
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		leoAssert(m_lLockRunningCount >= 0);

		lResult = ++m_lLockRunningCount;
	}

	if (lResult == 1)
	{
		// Wake-up the thread (if it's running at all) so it switches from waiting on the idle timeout to infinite-waiting.
		// This is done outside of m_cs so that the thread won't be blocked by us if the event wakes it immediately.
		// This switchover should not be left to happen naturally (i.e. after the idle timeout) as the change also triggers
		// events/callbacks which may need to do things as a result of the transition.
		::SetEvent(m_hEventWake);
	}

	return lResult;
}

// Each call to LockRunning should be balanced by a call to UnlockRunning.
// Once the thread, if running at all, empties its queue and is no longer locked it still stop running after the idle timeout expires.
LONG LeoHelpers::WorkerThread::UnlockRunning()
{
	LONG lResult = -1000;

	leoAssert(initOkay());

	if (initOkay())
	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		leoAssert(m_lLockRunningCount >= 1);

		lResult = --m_lLockRunningCount;
	}

	if (lResult == 0)
	{
		// Wake-up the thread (if it's running at all) so it switches from infinite-waiting to waiting for the idle timeout.
		// This is done outside of m_cs so that the thread won't be blocked by us if the event wakes it immediately.
		::SetEvent(m_hEventWake);
	}

	return lResult;
}

DWORD LeoHelpers::WorkerThread::GetIdleTimeout() const
{
	leoAssert(initOkay());

	LeoHelpers::CriticalSectionScoper css(&m_cs);

	return m_dwIdleTimeoutMS;
}

void LeoHelpers::WorkerThread::SetIdleTimeout(DWORD dwIdleTimeoutMS)
{
	leoAssert(initOkay());

	{
		LeoHelpers::CriticalSectionScoper css(&m_cs);

		m_dwIdleTimeoutMS = dwIdleTimeoutMS;
	}

	// Wake-up the thread (if it's running at all) so it picks up the new timeout (and no resets the wait time, too).
	// This is done outside of m_cs so that the thread won't be blocked by us if the event wakes it immediately.
	::SetEvent(m_hEventWake);
}

/*
bool LeoHelpers::WaitAndProcessSent(const int cHandles, HANDLE *pWaitHandles)
{
	DWORD dwWaitRes;
	MSG msg;

	while(true)
	{
		while (::PeekMessage(&msg, NULL, 0, 0, PM_REMOVE | PM_QS_SENDMESSAGE))
		{
			if (msg.message == WM_QUIT)
			{
				::PostQuitMessage(static_cast<int>(msg.wParam));
				return false;
			}
			else
			{
				::TranslateMessage(&msg);
				::DispatchMessage(&msg);
			}
		}

		// Wait for ready event or sent message from another thread.
		dwWaitRes = ::MsgWaitForMultipleObjects(cHandles, pWaitHandles, FALSE, INFINITE, QS_SENDMESSAGE);

		if (dwWaitRes == WAIT_OBJECT_0+cHandles)
		{
			continue; // Jump straight to message processing.
		}
		else if (dwWaitRes >= WAIT_OBJECT_0 && dwWaitRes < WAIT_OBJECT_0+cHandles)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
*/

bool LeoHelpers::CreateProcessWrapper(const wchar_t *szExePath, const wchar_t *szArgsWithoutExe, const wchar_t *szCurDir, bool bExeDirAsCurDir, HANDLE *phProcess)
{
	if (phProcess != NULL)
	{
		*phProcess = NULL;
	}

	// Convention requires that the exe name be explicitly included (in quotes) as the first argument.
	// (This is true even when giving CreateProcess a separate exe string.)
	std::wstring strArgs = L"\"";
	strArgs += szExePath;
	strArgs += L"\"";

	if (szArgsWithoutExe != NULL && szArgsWithoutExe[0] != L'\0')
	{
		strArgs += L" ";
		strArgs += szArgsWithoutExe;
	}

	// Unicode CreateProcess is stupid and really needs a non-const argument string as it may stupidly modify it in-place because it is stupid.
	LeoHelpers::WCharArrayScoper scArgsTemp(new wchar_t[strArgs.length() + 1]);
	wcscpy_s(scArgsTemp.Get(), strArgs.length() + 1, strArgs.c_str());

	// If the caller asked to use the target exe's dir as the CD, get it.
	std::wstring strCurDirTemp;
	if (bExeDirAsCurDir)
	{
		assert(szCurDir == NULL);

		LeoHelpers::GetParentPathString(&strCurDirTemp, szExePath);
		szCurDir = strCurDirTemp.c_str();

		assert(!strCurDirTemp.empty()); // Makes no sense asking to use the exe's dir then giving an exe with no path.
	}
	
	// Explicitly get the env-block as wide-chars so we know CREATE_UNICODE_ENVIRONMENT is appropriate.
	// (Otherwise, as I read things, to pass NULL and inherit our own env-block we have to know whether or not our parent process
	// used a Unicode block when it created us, which I see no way of knowing. Not sure if it's the API or MSDN being stupid there.)
	LeoHelpers::EnvBlockWideScoper envBlockW;

	if (!envBlockW.IsOk())
	{
		return false;
	}

	STARTUPINFO startupInfo = {0};
	startupInfo.cb = sizeof(startupInfo);
	PROCESS_INFORMATION processInfo = {0};

	if (!::CreateProcess(szExePath, scArgsTemp.Get(), NULL, NULL, FALSE, CREATE_UNICODE_ENVIRONMENT, envBlockW.GetEnvStringsW(), NULL, &startupInfo, &processInfo))
	{
		return false;
	}

	::CloseHandle(processInfo.hThread);

	if (phProcess != NULL)
	{
		*phProcess = processInfo.hProcess;
	}
	else
	{
		::CloseHandle(processInfo.hProcess);
	}

	return true;
}
