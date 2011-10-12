#include "stdafx.h"
#include "LeoHelpers_String.h"
#include "LeoHelpers_File.h"

bool LeoHelpers::GetShortFilePath(std::wstring *pstrResult, const wchar_t *szInput)
{
	bool bResult = false;

	wchar_t szDummy[1]; // Just in case GetShortPath is stupid.

	DWORD dwRes = ::GetShortPathName(szInput, szDummy, 0);

	if (dwRes > 0)
	{
		wchar_t *szBuf = new(std::nothrow) wchar_t[dwRes + 1];

		if (szBuf)
		{
			DWORD dwRes2 = ::GetShortPathName(szInput, szBuf, dwRes);

			if (dwRes2 <= dwRes)
			{
				*pstrResult = szBuf;
				bResult = true;
			}

			delete[] szBuf;
		}
	}

	return bResult;
}

#ifdef UrlCreateFromPath
bool LeoHelpers::UrlFromPath(std::wstring *pstrResult, const wchar_t *szPath)
{
	bool bResult = false;

	DWORD dwUrlBufferLen = 5120;
	wchar_t *szUrlBuffer = new wchar_t[dwUrlBufferLen];

	if (SUCCEEDED(::UrlCreateFromPath(szPath, szUrlBuffer, &dwUrlBufferLen, NULL)))
	{
		*pstrResult = szUrlBuffer;
		bResult = true;
	}
	else
	{
		pstrResult->clear();
	}

	delete[] szUrlBuffer;

	return bResult;
}
#endif

#ifdef UrlCreateFromPath
bool LeoHelpers::UrlFromPathShortIfPossible(std::wstring *pstrResult, const wchar_t *szPath, bool bOnlyTryShortIfNonAscii)
{
	std::wstring strPathToConvert;

	if ((bOnlyTryShortIfNonAscii && LeoHelpers::IsAllAscii(szPath, true))
	||	!LeoHelpers::GetShortFilePath(&strPathToConvert, szPath))
	{
		strPathToConvert = szPath;
	}

	return LeoHelpers::UrlFromPath(pstrResult, strPathToConvert.c_str());
}
#endif

/*
void LeoHelpers::EncodeUri(std::wstring *pstrResult, const wchar_t *szInput, bool bAppend, bool bConvertColon, bool bProperUTF8Method)
{
	if (!bAppend)
	{
		pstrResult->clear();
	}

	// See http://en.wikipedia.org/wiki/Percent-encoding
	// We encode the "reserved" characters as well since if they are part of a filename then they are not
	// being used for their "reserved" purpose and thus must be encoded.
	// Note that there is another copy of this table below.
	const static char *szNoEscapeSortedAnsi = "-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz~";
	const static size_t nesLenAnsi = strlen(szNoEscapeSortedAnsi);

	const static wchar_t *szNoEscapeSortedWide = L"-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz~";
	const static size_t nesLenWide = wcslen(szNoEscapeSortedWide);

#ifdef _DEBUG
	assert(nesLenAnsi == nesLenWide);

	for (size_t i = 1; i < nesLenAnsi; ++i)
	{
		assert(szNoEscapeSortedAnsi[i - 1] < szNoEscapeSortedAnsi[i]);
	}

	for (size_t i = 1; i < nesLenWide; ++i)
	{
		assert(szNoEscapeSortedWide[i - 1] < szNoEscapeSortedWide[i]);
	}
#endif

	if (bProperUTF8Method)
	{
		char szNumBuf[8];

		char *szUtf8Input = NULL;

		if (0 == WCtoMB(&szUtf8Input, szInput, -1, CP_UTF8))
		{
			*pstrResult = szInput; // Fail.
		}
		else
		{
			std::string strAnsiResult;

			const size_t slen = strlen(szUtf8Input);

			for(size_t i = 0; i < slen; ++i)
			{
				const char c = szUtf8Input[i];

				if (c == '\\')
				{
					strAnsiResult.append(1, '/'); // Convert backslashes to forwardslashes.
				}
				else if (bConvertColon && c == ':')
				{
					strAnsiResult.append(1, '|');
				}
				else if (c == ':' || std::binary_search(szNoEscapeSortedAnsi, szNoEscapeSortedAnsi + nesLenAnsi, c))
				{
					strAnsiResult.append(1, c);
				}
				else
				{
					unsigned char uc = c;

					_snprintf_s(szNumBuf, _countof(szNumBuf), _TRUNCATE, "%%%02X", static_cast< int >(uc));
					strAnsiResult.append(szNumBuf);
				}
			}

			wchar_t *wszResult = NULL;

			if (0 == MBtoWC(&wszResult, strAnsiResult.c_str(), -1, CP_UTF8))
			{
				*pstrResult = szInput; // Fail.
			}
			else
			{
				pstrResult->append(wszResult);

				delete [] wszResult;
			}

			delete[] szUtf8Input;
		}
	}
	else
	{
		wchar_t szNumBuf[8];

		const size_t slen = wcslen(szInput);

		for(size_t i = 0; i < slen; ++i)
		{
			const wchar_t c = szInput[i];

			if (c == L'\\')
			{
				pstrResult->append(1, L'/'); // Convert backslashes to forwardslashes.
			}
			else if (bConvertColon && c == L':')
			{
				pstrResult->append(1, L'|');
			}
			else if (c == L':' || std::binary_search(szNoEscapeSortedWide, szNoEscapeSortedWide + nesLenWide, c))
			{
				pstrResult->append(1, c);
			}
			else
			{
				_snwprintf_s(szNumBuf, _countof(szNumBuf), _TRUNCATE, (c <= 0xFF) ? L"%%%02hX" : L"%%%04hX", c);
				pstrResult->append(szNumBuf);
			}
		}
	}
}
*/

void LeoHelpers::GetParentPathString(std::wstring *pstrResult)
{
	assert(NULL != pstrResult);

	if (NULL == pstrResult)
	{
		return;
	}

	LeoHelpers::TrimTrailingSlashes(*pstrResult);

	std::wstring::size_type lastPos = pstrResult->find_last_of(L"\\/");

	if (lastPos == std::wstring::npos)
	{
		pstrResult->clear();
	}
	else
	{
		pstrResult->erase(lastPos);

		LeoHelpers::TrimTrailingSlashes(*pstrResult);
	}
}

std::wstring *LeoHelpers::GetParentPathString(std::wstring *pstrResult, const wchar_t *path)
{
	assert(NULL != pstrResult && NULL != path);

	if (NULL == pstrResult || NULL == path)
	{
		return NULL;
	}

	// Trim trailing slashes so paths like C:\Moo\Cow\ turn into C:\Moo not C:\Moo\Cow.

	*pstrResult = path;

	GetParentPathString(pstrResult);

	return pstrResult;
}

std::wstring *LeoHelpers::AppendPathString(std::wstring *pstrPath, const wchar_t *szAppendage)
{
	if (pstrPath == NULL)
	{
		return NULL;
	}

	if (szAppendage != NULL)
	{
		while (szAppendage[0] == L'\\' || szAppendage[0] == L'/')
		{
			++szAppendage;
		}
	}

	if (szAppendage != NULL && szAppendage[0] != L'\0')
	{
		if (!pstrPath->empty())
		{
			wchar_t lastChar = (*pstrPath)[pstrPath->length() - 1];

			if (lastChar != L'\\'
			&&	lastChar != L'/')
			{
				pstrPath->append(L"\\");
			}
		}

		pstrPath->append(szAppendage);
	}

	return pstrPath;
}

const wchar_t *LeoHelpers::GetLastPathPart(const wchar_t *szIn)
{
	if (NULL == szIn)
	{
		return(szIn);
	}

	size_t len = wcslen(szIn);

	if (1 >= len)
	{
		return(szIn);
	}

	const wchar_t *szOut = szIn + (len - 1);

	// Ignore any slashes at the very end of the string.
	while (szIn < szOut && (*szOut == L'\\' || *szOut == L'/'))
	{
		szOut--;
	}

	// Stop at the next slash we find.
	while (szIn < szOut && (*szOut != L'\\' && *szOut != L'/'))
	{
		szOut--;
	}

	if (*szOut == L'\\' || *szOut == L'/')
	{
		szOut++;
	}

	return(szOut);
}

const wchar_t *LeoHelpers::GetExtensionPart(bool bIncludeDot, const wchar_t *szIn)
{
	if (NULL == szIn)
	{
		return(szIn);
	}

	size_t len = wcslen(szIn);

	if (1 >= len)
	{
		return(NULL);
	}

	const wchar_t *szOut = szIn + (len - 1);

	while (szIn < szOut && (*szOut != L'.'))
	{
		if (*szOut == L'\\'
		||	*szOut == L'/'
		||	*szOut == L':')
		{
			return NULL; // No dot in the last path component.
		}

		szOut--;
	}

	if (szIn == szOut // Don't consider filenames like ".hidden" to have an extension.
	||	*szOut != L'.')
	{
		return(NULL);
	}

	if (szOut[1] == L'\0')
	{
		return(NULL);
	}

	if (!bIncludeDot)
	{
		szOut++;
	}

	return(szOut);
}

// Use GetLastError on failure.
bool LeoHelpers::CreateDirectoryAndParents(const wchar_t *szPath)
{
	if (szPath == NULL || *szPath == L'\0')
	{
		::SetLastError(ERROR_INVALID_NAME);
		return false;
	}

	if (::CreateDirectory(szPath, NULL))
	{
		return true;
	}

	DWORD dwErr = ::GetLastError();

	if (dwErr == ERROR_ALREADY_EXISTS)
	{
		return true;
	}

	if (dwErr != ERROR_PATH_NOT_FOUND)
	{
		return false;
	}

	std::wstring strParent;
	LeoHelpers::GetParentPathString(&strParent, szPath);

	if (strParent.empty())
	{
		::SetLastError(ERROR_PATH_NOT_FOUND); // In case GetParentPathString changed it.
		return false;
	}

	// Recursive call.
	if (!LeoHelpers::CreateDirectoryAndParents(strParent.c_str()))
	{
		return false;
	}

	if (::CreateDirectory(szPath, NULL))
	{
		return true;
	}

	return false;
}

UINT LeoHelpers::GetDriveTypeOfFolder(const wchar_t *szPath)
{
	// Should be correct for the folder even if the path contains soft-links etc.
	// Currently, we rely on undocumented behaviour of the OS for that to happen.
	// If that turns out to be unreliable we could manually check the path for
	// soft-links or whatever, but it doesn't seem to be necessary.

	if (szPath == NULL || *szPath == L'\0')
	{
		::SetLastError(ERROR_INVALID_NAME);
		return DRIVE_NO_ROOT_DIR;
	}

	std::wstring strPath = szPath;
	LeoHelpers::TrimTrailingSlashes(strPath);

	if (strPath.empty())
	{
		return DRIVE_NO_ROOT_DIR;
	}

	// MSDN (currently) says GetDriveType has to be given a path root
	// but, so long as there's a trailing slash, it seems to work with
	// any directory path. Even better, it correctly returns the drive
	// type for that directory even if it differs from the path's root's
	// type (e.g. due to a softlink along the way). However, since this
	// is undocumented and thus not guaranteed, we will eventually fall
	// back on the root path if the calls keep failing.

	// Keep calling again on the parent of the path in case:
	// 1) The passed path was to a file not a directory.
	// 2) The passed path did not exist yet (but some ancestor does).
	// 3) The OS is strict about GetDrivePath being given a root.

	while(!strPath.empty())
	{
		strPath += L'\\'; // Require one trailing space.

		UINT uiRes = ::GetDriveType(strPath.c_str());

		if (uiRes != DRIVE_NO_ROOT_DIR)
		{
			return uiRes;
		}

		std::wstring strTemp = strPath;
		LeoHelpers::GetParentPathString(&strPath, strTemp.c_str());
	}

	return DRIVE_NO_ROOT_DIR;
}

bool LeoHelpers::GetTempPath(std::wstring *pstrTempPath)
{
	// If we ask for the size first then we have to worry about the temp dir
	// being redefined between calls. In theory we still have to worry about
	// that with the code below but only if the temp path is already
	// longer than MAX_PATH and it then gets redefined, between calls, and
	// the new definition is even longer than the old one. In which case we
	// will fail gracefully. Since a lot of other software will fail just because
	// the temp path is longer than MAX_PATH it doesn't seem worth looping to
	// handle the worst case scenario.

	DWORD dwTempBufferSize = MAX_PATH;
	wchar_t szTempPath1[MAX_PATH];

	pstrTempPath->clear();

	DWORD dwTempRes = ::GetTempPath(dwTempBufferSize, szTempPath1);

	if (dwTempRes == 0)
	{
		return false; // last error already set.
	}
	else if (dwTempRes < dwTempBufferSize)
	{
		*pstrTempPath = szTempPath1;
		return true;
	}

	dwTempBufferSize = dwTempRes + 1;
	wchar_t *szTempPath2 = new wchar_t[dwTempBufferSize];

	dwTempRes = ::GetTempPath(dwTempBufferSize, szTempPath2);

	DWORD dwErr = ::GetLastError();

	bool bResult = false;

	if (dwTempRes != 0 && dwTempRes < dwTempBufferSize)
	{
		*pstrTempPath = szTempPath2;
		bResult = true;
	}

	delete[] szTempPath2;

	if (!bResult)
	{
		::SetLastError(dwErr);
	}

	return bResult;
}

// If bDontReallyOpen is true then the function won't really create a file and always returns INVALID_HANDLE_VALUE, but will still set *pstrFilepathOut to a name that is similar to what you'd get normally.
// On failure *pstrFilepathOut will be left empty so you can check that when bDontReallyOpen is true.
// If you set bDontReallyOpen then you absolutely must not create a file with the generated path because such a file may already exist and be used for other purposes. The generated path should only be used as a sample, for example if you want to test whether calling this function for real will result in a path that contains any non-ASCII characters.
HANDLE LeoHelpers::OpenTempFileNamePreserveExtension(const wchar_t *szTempPath, const wchar_t *szPrefix, const wchar_t *szFilenameOrDotExtIn, std::wstring *pstrFilepathOut, bool bDontReallyOpen, DWORD dwShareMode, DWORD dwFlagsAndAttributes, bool bTidyMode)
{
	HANDLE hFileResult = INVALID_HANDLE_VALUE;

	if (NULL != pstrFilepathOut)
	{
		pstrFilepathOut->clear();
	}

	std::wstring strFileStart;

	if (szTempPath != NULL)
	{
		strFileStart = szTempPath;
	}
	else if (!LeoHelpers::GetTempPath(&strFileStart))
	{
		return hFileResult; // last error already set by GetTempPath.
	}

	if (strFileStart.length() > 0 && '\\' != *(strFileStart.rbegin()) && '/' != *(strFileStart.rbegin()))
	{
		strFileStart += '\\';
	}

	strFileStart += szPrefix;

	std::wstring strExtension = L".tmp";

	if (NULL != szFilenameOrDotExtIn)
	{
		const wchar_t *szExt = wcsrchr(szFilenameOrDotExtIn, L'.');

		if (NULL != szExt)
		{
			strExtension = szExt;
		}
	}

	SYSTEMTIME systime;
	GetSystemTime(&systime);

	int i1 = systime.wYear;
	int i2 = systime.wMonth;
	int i3 = systime.wDay;
	int i4 = systime.wHour;
	int i5 = systime.wMinute;
	int i6 = systime.wSecond;
	int i7a = systime.wMilliseconds;

	DWORD dwErr = ::GetLastError();

	for (int i = 0; i < 100; i++)
	{
		wchar_t *szFilepath = NULL;

		if (bTidyMode)
		{
			if (i == 0)
			{
				szFilepath = StringAllocAndFormat(L"%s%s", strFileStart.c_str(), strExtension.c_str());
			}
			else
			{
				int idx = i - 1;
				szFilepath = StringAllocAndFormat(L"%s%d%s", strFileStart.c_str(), idx, strExtension.c_str());

				if (i >= 20)
				{
					bTidyMode = false; // Turn off tidy mode if we're having trouble finding a unique name.
				}
			}
		}
		else
		{
			int i7 = i7a + i;

			szFilepath = StringAllocAndFormat(L"%s%04d%02d%02d%02d%02d%02d%04d%s",
							strFileStart.c_str(), i1,i2,i3,i4,i5,i6,i7, strExtension.c_str());
		}

		if (NULL == szFilepath)
		{
			dwErr = ERROR_NOT_ENOUGH_MEMORY;
			break;
		}
		else
		{
			if (!bDontReallyOpen)
			{
				hFileResult = ::CreateFile(szFilepath, GENERIC_WRITE, dwShareMode, 0, CREATE_NEW, dwFlagsAndAttributes, 0);
				dwErr = ::GetLastError();
			}

			if (bDontReallyOpen || hFileResult != INVALID_HANDLE_VALUE)
			{
				if (NULL != pstrFilepathOut)
				{
					*pstrFilepathOut = szFilepath;
				}
				delete [] szFilepath;
				break;
			}
			delete [] szFilepath;

			if (dwErr != ERROR_ALREADY_EXISTS && dwErr != ERROR_FILE_EXISTS) // CreateFile can return either, depending on the CREATE_NEW/CREATE_ALWAYS flag.
			{
				break;
			}
		}
	}

	if (hFileResult == INVALID_HANDLE_VALUE)
	{
		::SetLastError(dwErr);
	}

	return(hFileResult);
}

std::wstring LeoHelpers::GenerateSafeName(const wchar_t *szInput)
{
	std::wstring strResult;

	if (NULL != szInput)
	{
		while(*szInput)
		{
			if (iswalpha(*szInput) || iswdigit(*szInput))
			{
				strResult += *szInput++;
			}
			else
			{
				strResult += L"_";
				szInput++;
			}
		}
	}

	return strResult;
}

bool LeoHelpers::CopyFile(const wchar_t *szSource, const wchar_t *szDest, HANDLE hAbortEvent, bool bClearReadOnlyAttrib, bool bFailIfExists)
{
	class CopyFileDummyInner
	{
	public:
		static DWORD CALLBACK CopyProgressRoutine(
			LARGE_INTEGER TotalFileSize,
			LARGE_INTEGER TotalBytesTransferred,
			LARGE_INTEGER StreamSize,
			LARGE_INTEGER StreamBytesTransferred,
			DWORD dwStreamNumber,
			DWORD dwCallbackReason,
			HANDLE hSourceFile,
			HANDLE hDestinationFile,
			LPVOID lpData)
		{
			if (lpData == NULL)
			{
				return PROGRESS_QUIET;
			}

			if (WAIT_TIMEOUT != ::WaitForSingleObject(static_cast< HANDLE >(lpData), 0))
			{
				return PROGRESS_CANCEL;
			}

			return PROGRESS_CONTINUE;
		}
	};

	BOOL bCancel = FALSE; // Never used.
	DWORD dwCopyFlags = (bFailIfExists ? COPY_FILE_FAIL_IF_EXISTS : 0);

	if (0 == ::CopyFileEx(szSource, szDest, CopyFileDummyInner::CopyProgressRoutine, hAbortEvent, &bCancel, dwCopyFlags))
	{
		return false;
	}

	if (bClearReadOnlyAttrib)
	{
		DWORD dwAttribs = ::GetFileAttributes(szDest);

		if (dwAttribs != INVALID_FILE_ATTRIBUTES
		&&	(dwAttribs&FILE_ATTRIBUTE_READONLY))
		{
			dwAttribs -= FILE_ATTRIBUTE_READONLY;
			::SetFileAttributes(szDest, dwAttribs);
		}
	}

	return true;
}

bool LeoHelpers::MoveFile(const wchar_t *szSource, const wchar_t *szDest, HANDLE hAbortEvent, bool bFailIfExists)
{
	class MoveFileDummyInner
	{
	public:
		static DWORD CALLBACK CopyProgressRoutine(
			LARGE_INTEGER TotalFileSize,
			LARGE_INTEGER TotalBytesTransferred,
			LARGE_INTEGER StreamSize,
			LARGE_INTEGER StreamBytesTransferred,
			DWORD dwStreamNumber,
			DWORD dwCallbackReason,
			HANDLE hSourceFile,
			HANDLE hDestinationFile,
			LPVOID lpData)
		{
			if (lpData == NULL)
			{
				return PROGRESS_QUIET;
			}

			if (WAIT_TIMEOUT != ::WaitForSingleObject(static_cast< HANDLE >(lpData), 0))
			{
				return PROGRESS_CANCEL;
			}

			return PROGRESS_CONTINUE;
		}
	};

	BOOL bCancel = FALSE; // Never used.
	DWORD dwMoveFlags = MOVEFILE_COPY_ALLOWED | (bFailIfExists ? 0 : MOVEFILE_REPLACE_EXISTING);

	if (0 == ::MoveFileWithProgress(szSource, szDest, MoveFileDummyInner::CopyProgressRoutine, hAbortEvent, dwMoveFlags))
	{
		return false;
	}

	return true;
}

// Intended for reading from pipes.
bool LeoHelpers::ReadFileUntilDone(HANDLE hFile, LPVOID lpBuffer, DWORD dwNumberOfBytesToRead)
{
	BYTE *pBuffer = reinterpret_cast<BYTE *>(lpBuffer);

	DWORD dwBytesRead;

	while(dwNumberOfBytesToRead > 0)
	{
		dwBytesRead = 0;

		if (!ReadFile(hFile, pBuffer, dwNumberOfBytesToRead, &dwBytesRead, NULL)
		||	dwBytesRead == 0
		||	dwBytesRead > dwNumberOfBytesToRead)
		{
			return false;
		}

		dwNumberOfBytesToRead -= dwBytesRead;
		pBuffer += dwBytesRead;
	}

	return true;
}

// Intended for writing to pipes.
bool LeoHelpers::WriteFileUntilDone(HANDLE hFile, LPVOID lpBuffer, DWORD dwNumberOfBytesToWrite)
{
	BYTE *pBuffer = reinterpret_cast<BYTE *>(lpBuffer);

	DWORD dwBytesWritten;

	while(dwNumberOfBytesToWrite > 0)
	{
		dwBytesWritten = 0;

		if (!WriteFile(hFile, pBuffer, dwNumberOfBytesToWrite, &dwBytesWritten, NULL)
		||	dwBytesWritten == 0
		||	dwBytesWritten > dwNumberOfBytesToWrite)
		{
			return false;
		}

		dwNumberOfBytesToWrite -= dwBytesWritten;
		pBuffer += dwBytesWritten;
	}

	return true;
}


LeoHelpers::FileAndStream::FileAndStream(const wchar_t *szFilePath, HANDLE hAbortEvent, bool bOpenTempCopy)
: m_strFileName(LeoHelpers::GetLastPathPart(szFilePath))
, m_strFilePath(szFilePath)
, m_bDeleteFilePathOnClose(false)
, m_bOpenTempCopyFilePath(bOpenTempCopy)
, m_pStream(NULL)
, m_bNoRandomSeekInStream(false)
, m_bSlowStream(false)
, m_bSlowSeekStream(false)
, m_hAbortEvent(hAbortEvent)
, m_bStreamIsFile(false)
{
}

// pStream can be NULL but only if you are never going to call methods which read data or get the steam or file path.
LeoHelpers::FileAndStream::FileAndStream(const wchar_t *szFileName, IStream *pStream, bool bNoRandomSeek, bool bSlow, bool bSlowSeek)
: m_strFileName(LeoHelpers::GetLastPathPart(szFileName))
, m_bDeleteFilePathOnClose(false)
, m_bOpenTempCopyFilePath(false)
, m_pStream(pStream)
, m_bNoRandomSeekInStream(bNoRandomSeek)
, m_bSlowStream(bSlow)
, m_bSlowSeekStream(bSlowSeek)
, m_hAbortEvent(NULL)
, m_bStreamIsFile(false)
{
	if (m_pStream != NULL)
	{
		m_pStream->AddRef();
	}
}

LeoHelpers::FileAndStream::FileAndStream(const wchar_t *szFileName, const BYTE *pData, UINT cbDataSize)
: m_strFileName(LeoHelpers::GetLastPathPart(szFileName))
, m_bDeleteFilePathOnClose(false)
, m_bOpenTempCopyFilePath(false)
, m_pStream(NULL)
, m_bNoRandomSeekInStream(false)
, m_bSlowStream(false)
, m_bSlowSeekStream(false)
, m_hAbortEvent(NULL)
, m_bStreamIsFile(false)
{
	HGLOBAL hGlobal = ::GlobalAlloc(GMEM_MOVEABLE, cbDataSize);

	if (hGlobal != NULL)
	{
		void *pBuffer = ::GlobalLock(hGlobal);

		if (pBuffer != NULL)
		{
			::memcpy_s(pBuffer, cbDataSize, pData, cbDataSize);

			::GlobalUnlock(hGlobal);

			if (S_OK != ::CreateStreamOnHGlobal(hGlobal, TRUE, &m_pStream))
			{
				m_pStream = NULL;
			}
		}
	}
}

LeoHelpers::FileAndStream::~FileAndStream()
{
	if (m_pStream != NULL)
	{
		m_pStream->Release();
		m_pStream = NULL;
	}

	if (m_bDeleteFilePathOnClose && !m_strFilePath.empty())
	{
		for(int i = 0; i < 5; ++i)
		{
			if (DeleteFile(m_strFilePath.c_str()))
			{
				break;
			}

			Sleep(1000);
		}
	}

	m_bDeleteFilePathOnClose = false;
	m_bOpenTempCopyFilePath = false;
	m_bNoRandomSeekInStream = false;
	m_bSlowStream = false;
	m_bSlowSeekStream = false;
	m_bStreamIsFile = false;
	m_strFilePath.clear();
	m_strFileName.clear();
	m_hAbortEvent = NULL;
}

// This allows you to set the OpenTempCopy flag after construction.
// Any subsequent call to GetFilePath will result in the file being copied to a temp path if
// it had not already been done. Any preceeding call to GetFilePath may, of course, have alredy
// received the real filename. If the data is in a stream then calling this has no effect as
// GetFilePath will always cause the data to be saved to a temp file regardless of the flag.
void LeoHelpers::FileAndStream::SetOpenTempCopy(bool bOpenTemp)
{
	m_bOpenTempCopyFilePath = bOpenTemp;
}

// The file name may not match the name of the actual file. It is indicative of the original file's name
// but the file path returned by GetFilePath may be a temp-file with a different name. GetFileName should
// be used for display purposes and for any logic which depends on the original file's name.
// Calling GetFileName will not trigger any files or streams to be created.

bool LeoHelpers::FileAndStream::GetFileName(std::wstring *pstrFileName)
{
	if (m_strFileName.empty())
	{
		return false;
	}

	*pstrFileName = m_strFileName;
	return true;
}

bool LeoHelpers::FileAndStream::GetFileExtension(std::wstring *pstrFileExtension, bool bIncludeDot)
{
	if (m_strFileName.empty())
	{
		return false;
	}

	const wchar_t *szExt = LeoHelpers::GetExtensionPart(bIncludeDot, m_strFileName.c_str());

	if (szExt == NULL)
	{
		return false;
	}

	*pstrFileExtension = szExt;
	return true;
}

bool LeoHelpers::FileAndStream::HasFilePath()
{
	return !m_strFilePath.empty();
}

bool LeoHelpers::FileAndStream::HasStream()
{
	return m_pStream != NULL;
}

bool LeoHelpers::FileAndStream::GetNominalFilePath(std::wstring *pstrNominalFilePath)
{
	if (pstrNominalFilePath == NULL)
	{
		return false;
	}

	pstrNominalFilePath->clear();

	if (m_strFileName.empty())
	{
		return false;
	}

	if ((m_bOpenTempCopyFilePath && !m_bDeleteFilePathOnClose)
	||	(m_strFilePath.empty() && m_pStream != NULL))
	{
		return GetNominalTempFilePath(pstrNominalFilePath);
	}

	if (!m_strFilePath.empty())
	{
		*pstrNominalFilePath = m_strFilePath;
		return true;
	}

	return false;
}

bool LeoHelpers::FileAndStream::GetNominalTempFilePath(std::wstring *pstrNominalTempFilePath)
{
	if (pstrNominalTempFilePath == NULL)
	{
		return false;
	}

	pstrNominalTempFilePath->clear();

	if (m_strFileName.empty())
	{
		return false;
	}

	// Work out a path that will be similar to the real temp file path if the file ever gets written to disk.
	// We tell OpenTempFileNamePreserveExtension to not really create the file.
	LeoHelpers::OpenTempFileNamePreserveExtension(NULL, L"dnvt", m_strFileName.c_str(), pstrNominalTempFilePath, true);

	return !pstrNominalTempFilePath->empty();
}

// These calls will cause a file or stream to be created if there isn't one already.
// Call HasFilePath and HasStream to see what already exists if you can work with both and want
// to avoid the overhead of conversion.

#ifdef SHCreateStreamOnFile
bool LeoHelpers::FileAndStream::GetFilePath(std::wstring *pstrFilePath)
{
	bool bResult = false;

	if (PrepareFileForReading()
	||	WriteStreamToTempFile())
	{
		*pstrFilePath = m_strFilePath;

		bResult = true;
	}
/*
	if (m_bStreamIsFile && m_pStream != NULL)
	{
		m_pStream->Release();
		m_pStream = NULL;

		m_bNoRandomSeekInStream = false;
		m_bSlowStream = false;
		m_bSlowSeekStream = false;

		m_bStreamIsFile = false;
	}
*/
	return bResult;
}
#endif

// The stream will not be AddRef'd by this call. It will exist as long as the CFileAndSteam does
// so you should not normally need to AddRef it.
// If a stream is returned it will always be fast and seekable.
// Whenever GetStream is called the stream's position is reset to the start.

#ifdef SHCreateStreamOnFile
bool LeoHelpers::FileAndStream::GetStream_NoRef_Conversion_SeekReset(IStream **ppStream, bool *pOutNoRandomSeek, bool *pOutSlow, bool *pOutSlowSeek)
{
	assert(ppStream != NULL);

	bool bResult = false;

	if (m_pStream != NULL)
	{
		// Don't check m_bSlowSeekStream as it typically only indicates seeking is slow when going beyond what
		// has been read (extracted) so far from an archive, not all seeking in general.
		if ((!m_bNoRandomSeekInStream && !m_bSlowStream)
		||	(WriteStreamToTempFile() && m_pStream != NULL))
		{
			bResult = true;
		}
	 }
	 else if (PrepareFileForReading())
	 {
		m_bNoRandomSeekInStream = false;
		m_bSlowStream = false;
		m_bSlowSeekStream = false;

		if (S_OK != SHCreateStreamOnFile(m_strFilePath.c_str(), STGM_READ|STGM_SHARE_DENY_NONE, &m_pStream)
		||	m_pStream == NULL)
		{
			m_pStream = NULL;
		}
		else
		{
			m_bStreamIsFile = true;

			bResult = true;
		}
	}

	if (bResult)
	{
		LARGE_INTEGER liZero;
		liZero.QuadPart = 0;

		if (S_OK != m_pStream->Seek(liZero, STREAM_SEEK_SET, NULL))
		{
			bResult = false;
		}
	}

	if (bResult)
	{
		*ppStream = m_pStream;

		if (pOutNoRandomSeek != NULL) { *pOutNoRandomSeek = m_bNoRandomSeekInStream; }
		if (pOutSlow         != NULL) { *pOutSlow         = m_bSlowStream;           }
		if (pOutSlowSeek     != NULL) { *pOutSlowSeek     = m_bSlowSeekStream;       }

		return true;
	}
	else
	{
		*ppStream = NULL;

		if (pOutNoRandomSeek != NULL) { *pOutNoRandomSeek = false; }
		if (pOutSlow         != NULL) { *pOutSlow         = false; }
		if (pOutSlowSeek     != NULL) { *pOutSlowSeek     = false; }

		return false;
	}
}
#endif

// Call this to get the current IStream, if any.
// Unlike the GetStream() method:
// - The returned IStream *will* be AddRef'd and you must Release it.
// - The returned IStream may not be seekable.
// - If the object contains a filepath that hasn't already been converted into an IStream then that conversion will not happen and the method will fail instead.
// - The returned IStream will not have its position reset to the start of the IStream (since it may not be seekable).

bool LeoHelpers::FileAndStream::GetStream_AddRef_NoConversion_NoSeekReset(IStream **ppStream, bool *pOutNoRandomSeek, bool *pOutSlow, bool *pOutSlowSeek)
{
	assert(ppStream != NULL);

	*ppStream = m_pStream;

	if (*ppStream)
	{
		(*ppStream)->AddRef();

		if (pOutNoRandomSeek != NULL) { *pOutNoRandomSeek = m_bNoRandomSeekInStream; }
		if (pOutSlow         != NULL) { *pOutSlow         = m_bSlowStream;           }
		if (pOutSlowSeek     != NULL) { *pOutSlowSeek     = m_bSlowSeekStream;       }

		return true;
	}
	else
	{
		if (pOutNoRandomSeek != NULL) { *pOutNoRandomSeek = false; }
		if (pOutSlow         != NULL) { *pOutSlow         = false; }
		if (pOutSlowSeek     != NULL) { *pOutSlowSeek     = false; }

		return false;
	}
}

bool LeoHelpers::FileAndStream::PrepareFileForReading()
{
	bool bResult = false;

	if (!m_strFileName.empty()
	&&	!m_strFilePath.empty())
	{
		if (!m_bOpenTempCopyFilePath || m_bDeleteFilePathOnClose)
		{
			bResult = true;
		}
		else
		{
			// Copy the file to a temporary file and refer to that from now on.

			std::wstring strTempFilePath;

			HANDLE hTempFile = OpenTempFileNamePreserveExtension(NULL, L"dnvt", m_strFileName.c_str(), &strTempFilePath, false);

			if (hTempFile != INVALID_HANDLE_VALUE)
			{
				CloseHandle(hTempFile);

				if (!LeoHelpers::CopyFile(m_strFilePath.c_str(), strTempFilePath.c_str(), m_hAbortEvent, true, false))
				{
					DeleteFile(strTempFilePath.c_str());
				}
				else
				{
					m_bOpenTempCopyFilePath = false;
					m_bDeleteFilePathOnClose = true;
					m_strFilePath = strTempFilePath;

					bResult = true;
				}
			}
		}
	}

	return bResult;
}

#ifdef SHCreateStreamOnFile
bool LeoHelpers::FileAndStream::WriteStreamToTempFile()
{
	bool bResult = false;

	if (m_pStream != NULL && !m_strFileName.empty())
	{
		std::wstring strTempFilePath;

		HANDLE hTempFile = OpenTempFileNamePreserveExtension(NULL, L"dnvt", m_strFileName.c_str(), &strTempFilePath, false);

		if (hTempFile != INVALID_HANDLE_VALUE)
		{
			bool bWriteError = false;
			DWORD dwSize;
			BYTE bBuf[8192];

			LARGE_INTEGER liZero;
			ULARGE_INTEGER uliPrevious;

			liZero.QuadPart = 0;
			uliPrevious.QuadPart = 0;

			if (!m_bNoRandomSeekInStream)
			{
				m_pStream->Seek(liZero, STREAM_SEEK_CUR, &uliPrevious);
				m_pStream->Seek(liZero, STREAM_SEEK_SET, NULL);
			}

			while (S_OK == m_pStream->Read(bBuf, sizeof(bBuf), &dwSize))
			{
				if (0 == dwSize)
				{
					// Some IStream implementations return S_OK and set dwSize to zero to indicate
					// the end of the stream. (Some use S_FALSE instead.)
					break;
				}

				if (0 < dwSize)
				{
					DWORD dwTemp = 0;

					if (0 == WriteFile(hTempFile,bBuf,dwSize,&dwTemp,0))
					{
						bWriteError = true;
						break;
					}
				}
			}

			if (!m_bNoRandomSeekInStream)
			{
				// When using STREAM_SEEK_SET the first argument to Seek is treated as unsigned.

				LARGE_INTEGER liPrevious;
				liPrevious.LowPart  = uliPrevious.LowPart;
				liPrevious.HighPart = uliPrevious.HighPart;

				m_pStream->Seek(liPrevious, STREAM_SEEK_SET, NULL);
			}

			// Close temporary file
			CloseHandle(hTempFile);

			if (bWriteError)
			{
				DeleteFile(strTempFilePath.c_str());
			}
			else
			{
				m_bOpenTempCopyFilePath = false;
				m_bDeleteFilePathOnClose = true;
				m_strFilePath = strTempFilePath;

				m_pStream->Release();
				m_bStreamIsFile = false;
				m_bNoRandomSeekInStream = false;
				m_bSlowStream = false;
				m_bSlowSeekStream = false;

				if (S_OK != SHCreateStreamOnFile(m_strFilePath.c_str(), STGM_READ|STGM_SHARE_DENY_NONE, &m_pStream))
				{
					m_pStream = NULL;
				}
				else
				{
					m_bStreamIsFile = true;
					bResult = true;
				}
			}
		}
	}

	return bResult;
}
#endif

void LeoHelpers::DirEnum::Close()
{
	if (m_hFind != INVALID_HANDLE_VALUE)
	{
		::FindClose(m_hFind);
		m_hFind = INVALID_HANDLE_VALUE;
	}
	m_strDirPath.clear();
}

bool LeoHelpers::DirEnum::Init(DWORD *pdwError, const wchar_t *szDirPath, const wchar_t *szPattern)
{
	assert(pdwError != NULL && szDirPath != NULL); // szPattern may be NULL.

	Close();
	ZeroMemory(&m_wfd, sizeof(m_wfd));

	m_strDirPath = szDirPath;

	std::wstring strSearch = szDirPath;
	LeoHelpers::AppendPathString(&strSearch, (szPattern != NULL) ? szPattern : L"*");
	m_hFind = ::FindFirstFile(strSearch.c_str(), &m_wfd);

	if (m_hFind != INVALID_HANDLE_VALUE && skipDots())
	{
		return true;
	}

	DWORD dwError = ::GetLastError();

	Close(); // Needed if the skipDots() call was where we failed; e.g. due to an empty dir.

	if (dwError == ERROR_FILE_NOT_FOUND)
	{
		return true;
	}

	*pdwError = dwError;
	return false;
}

void LeoHelpers::DirEnum::CurrentFullPath(std::wstring *pStr) const
{
	assert(!AtEnd());

	(*pStr) = m_strDirPath;
	LeoHelpers::AppendPathString(pStr, m_wfd.cFileName);
}

bool LeoHelpers::DirEnum::Step(DWORD *pdwError)
{
	assert(pdwError != NULL);

	if (m_hFind == INVALID_HANDLE_VALUE)
	{
		*pdwError = ERROR_INVALID_HANDLE;
		return false;
	}

	if (::FindNextFile(m_hFind, &m_wfd) && skipDots())
	{
		return true;
	}

	DWORD dwError = ::GetLastError();

	Close();

	if (dwError == ERROR_NO_MORE_FILES)
	{
		return true;
	}

	*pdwError = dwError;
	return false;
}

bool LeoHelpers::DirEnum::skipDots()
{
	assert(m_hFind != INVALID_HANDLE_VALUE);

	while (m_wfd.cFileName[0] == L'.'
	&&	   (m_wfd.cFileName[1] == L'\0' || (m_wfd.cFileName[1] == L'.' && m_wfd.cFileName[2] == L'\0')))
	{
		if (!::FindNextFile(m_hFind, &m_wfd))
		{
			return false;
		}
	}

	return true;
}
