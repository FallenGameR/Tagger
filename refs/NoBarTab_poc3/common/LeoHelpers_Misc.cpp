#include "stdafx.h"
#include "LeoHelpers_String.h"
#include "LeoHelpers_Misc.h"

bool LeoHelpers::IsWindowsVersionGreaterOrEqual(DWORD dwMaj, DWORD dwMin, WORD wSPMaj, WORD wSPMin)
{
	OSVERSIONINFOEX osVersionInfoEx = {0};
	osVersionInfoEx.dwOSVersionInfoSize = sizeof(osVersionInfoEx);

	osVersionInfoEx.dwMajorVersion    = dwMaj;  // Windows Server 2003, Windows XP, or Windows 2000
	osVersionInfoEx.dwMinorVersion    = dwMin;  // Windows XP
	osVersionInfoEx.wServicePackMajor = wSPMaj; // Usually don't care about service pack but docs say we...
	osVersionInfoEx.wServicePackMinor = wSPMin; // ...must test it if we test the major version.

	const DWORD dwTypeMask = VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR | VER_SERVICEPACKMINOR;

	DWORDLONG dwlConditionMask = 0;
	dwlConditionMask = ::VerSetConditionMask(dwlConditionMask, VER_MAJORVERSION,     VER_GREATER_EQUAL);
	dwlConditionMask = ::VerSetConditionMask(dwlConditionMask, VER_MINORVERSION,     VER_GREATER_EQUAL);
	dwlConditionMask = ::VerSetConditionMask(dwlConditionMask, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);
	dwlConditionMask = ::VerSetConditionMask(dwlConditionMask, VER_SERVICEPACKMINOR, VER_GREATER_EQUAL);

	if (::VerifyVersionInfo(&osVersionInfoEx, dwTypeMask, dwlConditionMask))
	{
		return true;
	}

	return false;
}


#ifndef SPI_GETCLIENTAREAANIMATION
#define SPI_GETCLIENTAREAANIMATION          0x1042
#endif

bool LeoHelpers::WantWindowAnimations()
{
	if (LeoHelpers::IsWindowsVistaOrAbove())
	{
		BOOL fAnimations = TRUE;

		if (::SystemParametersInfo(SPI_GETCLIENTAREAANIMATION, 0, &fAnimations, 0))
		{
			return fAnimations ? true : false; // Note: MSDN documents this the wrong way around at the time of writing.
		}
	}

	return ::GetSystemMetrics(SM_REMOTESESSION) ? false : true;
}

bool LeoHelpers::WantFullWindowDragging()
{
	BOOL fFullWindowDragging = TRUE;

	if (::SystemParametersInfo(SPI_GETDRAGFULLWINDOWS, 0, &fFullWindowDragging, 0))
	{
		return fFullWindowDragging ? true : false;
	}

	return true;
}
