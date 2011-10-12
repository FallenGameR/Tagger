#pragma once

namespace LeoHelpers
{
	bool GetShortFilePath(std::wstring *pstrResult, const wchar_t *szInput);

	bool UrlFromPath(std::wstring *pstrResult, const wchar_t *szPath);

	bool UrlFromPathShortIfPossible(std::wstring *pstrResult, const wchar_t *szPath, bool bOnlyTryShortIfNonAscii);

//	void EncodeUri(std::wstring *pstrResult, const wchar_t *szInput, bool bAppend, bool bConvertColon, bool bProperUTF8Method);

	inline void TrimTrailingSlashes(std::wstring &strPath)
	{
		std::wstring::size_type len = strPath.length();

		while(len > 0 && (strPath[len - 1] == L'\\' || strPath[len - 1] == L'/'))
		{
			--len;
			strPath.erase(len);
		}
	}

	// A path with no slashes or the empty string returns an empty string.
	void GetParentPathString(std::wstring *pstrResult);
	std::wstring *GetParentPathString(std::wstring *pstrResult, const wchar_t *path);

	std::wstring *AppendPathString(std::wstring *pstrPath, const wchar_t *szAppendage);

	const wchar_t *GetLastPathPart(const wchar_t *szIn);
	inline wchar_t *GetLastPathPart(wchar_t *szIn) { return const_cast< wchar_t * >( GetLastPathPart(const_cast< const wchar_t * >( szIn )) ); }

	const wchar_t *GetExtensionPart(bool bIncludeDot, const wchar_t *szIn);

	inline const wchar_t *GetExtensionPart(bool bIncludeDot, const std::wstring &strIn)
	{
		std::wstring::size_type dotPos = strIn.find_last_of(L'.');

		if (dotPos == 0 // Don't consider filenames like ".hidden" to have an extension.
		||	dotPos == std::wstring::npos
		||	dotPos == (strIn.length() - 1))
		{
			return NULL;
		}

		return strIn.c_str() + dotPos + (bIncludeDot ? 0 : 1);
	}

	inline void TrimExtensionPart(bool bTrimDot, std::wstring *pStr)
	{
		std::wstring::size_type dotPos = pStr->find_last_of(L'.');

		if (dotPos == 0 // Don't consider filenames like ".hidden" to have an extension.
		||	dotPos == std::wstring::npos
		||	dotPos == (pStr->length() - 1))
		{
			return;
		}

		pStr->erase(dotPos + (bTrimDot ? 0 : 1));
	}

	// For code that makes file *paths* legal, Opus7Zip has a function which could be made more generic and
	// pulled into here if needed in other places.
	inline bool IsLegalPathChar(const wchar_t &c, bool bAllowPathSeps, bool bAllowDots, bool bAllowColons)
	{
		switch(c)
		{
		default:
			return true;
		case L'\\':
		case L'/':
			return bAllowPathSeps;
		case L'.':
			return bAllowDots;
		case L':':
			return bAllowColons;
		case L'*':
		case L'?':
		case L'"':
		case L'<':
		case L'>':
		case L'|':
			return false;
		}
	}

	// Use GetLastError on failure.
	bool CreateDirectoryAndParents(const wchar_t *szPath);

	// Should be correct for the folder even if the path contains soft-links etc.
	UINT GetDriveTypeOfFolder(const wchar_t *szPath);

	std::wstring GenerateSafeName(const wchar_t *szInput);

	bool GetTempPath(std::wstring *pstrTempPath);

	// TODO: Remove the argument defaults and consider setting the temporary-file attribute on most callers of this function, for better file caching.
	// If bDontReallyOpen is true then the function won't really create a file and always returns INVALID_HANDLE_VALUE, but will still set *pstrFilepathOut to a suitable filename.
	// On failure *pstrFilepathOut will be left empty so you can check that when bDontReallyOpen is true.
	// Of course, if you don't create the file then there is nothing to stop someone else (e.g. another caller of this function)
	// creating it unexpectedly, so the name should only be considered as an example (e.g. to see if we'll generate a path with any non-ASCII chars in it) and never really used.
	// Set bTidyMode when the tempfile is visible to the user (not hidden in a temp dir) and unlikely to clash with an existing file;
	// in tidy-mode the name will be tried as-is (prefix + extension of szFilenameIn, or ".tmp") and if that already exists a counter is added.
	HANDLE OpenTempFileNamePreserveExtension(const wchar_t *szTempPath, const wchar_t *szPrefix, const wchar_t *szFilenameOrDotExtIn, std::wstring *pstrFilepathOut, bool bDontReallyOpen, DWORD dwShareMode = 0, DWORD dwFlagsAndAttributes = FILE_ATTRIBUTE_NORMAL, bool bTidyMode = false);

	bool CopyFile(const wchar_t *szSource, const wchar_t *szDest, HANDLE hAbortEvent, bool bClearReadOnlyAttrib, bool bFailIfExists);
	bool MoveFile(const wchar_t *szSource, const wchar_t *szDest, HANDLE hAbortEvent, bool bFailIfExists);

	// Intended for reading from pipes.
	bool ReadFileUntilDone(HANDLE hFile, LPVOID lpBuffer, DWORD dwNumberOfBytesToRead);

	// Intended for writing to pipes.
	bool WriteFileUntilDone(HANDLE hFile, LPVOID lpBuffer, DWORD dwNumberOfBytesToWrite);

	class FileAndStream
	{
	public:
		// hAbortEvent can be null.
		FileAndStream(const wchar_t *szFilePath, HANDLE hAbortEvent, bool bOpenTempCopy);

		// pStream can be NULL but only if you are never going to call methods which read data or get the steam or file path.
		FileAndStream(const wchar_t *szFileName, IStream *pStream, bool bNoRandomSeek, bool bSlow, bool bSlowSeek);

		FileAndStream(const wchar_t *szFileName, const BYTE *pData, UINT cbDataSize);

		~FileAndStream(); // Warning: Non-virtual destructor.

	private:
		// disallow
		FileAndStream(const FileAndStream &rhs);
		FileAndStream &operator=(const FileAndStream &rhs);

	public:

		// This allows you to set the OpenTempCopy flag after construction.
		// Any subsequent call to GetFilePath will result in the file being copied to a temp path if
		// it had not already been done. Any preceeding call to GetFilePath may, of course, have alredy
		// received the real filename. If the data is in a stream then calling this has no effect as
		// GetFilePath will always cause the data to be saved to a temp file regardless of the flag.
		void SetOpenTempCopy(bool bOpenTemp);

		// The file name may not match the name of the actual file. It is indicative of the original file's name
		// but the file path returned by GetFilePath may be a temp-file with a different name. GetFileName should
		// be used for display purposes and for any logic which depends on the original file's name.
		// Calling GetFileName will not trigger any files or streams to be created.
		bool GetFileName(std::wstring *pstrFileName);
		bool GetFileExtension(std::wstring *pstrFileExtension, bool bIncludeDot);

		// GetNominalFilePath gets a full path that will be similar to, but may not be the same as,
		// the file path returned by GetFilePath. You can use this to test for non-ASCII characters
		// in the file path without triggering a stream to be written to a file as you would if you
		// called GetFilePath.
		bool GetNominalFilePath(std::wstring *pstrNominalFilePath);
		bool GetNominalTempFilePath(std::wstring *pstrNominalTempFilePath);

		bool HasFilePath();
		bool HasStream();

		// These return the current seeking ability. One reason to call these is to test whether or not calling GetStream_NoRef_Conversion_SeekReset will have to do an expensive extraction to create a seekable stream.
		bool HasFastRandomSeek() { return !m_bNoRandomSeekInStream && !m_bSlowStream && !m_bSlowSeekStream; } // Does not imply there is a stream (but it'll be cheap to create if there isn't one).
		bool HasNoRandomSeekStream() { return m_bNoRandomSeekInStream; }
		bool HasSlowStream()         { return m_bSlowStream; }
		bool HasSlowSeekStream()     { return m_bSlowSeekStream; }

		HANDLE GetAbortEvent() { return m_hAbortEvent; } // May return NULL if there isn't one.

		// These calls will cause a file or stream to be created if there isn't one already.
		// Call HasFilePath and HasStream to see what already exists if you can work with both and want
		// to avoid the overhead of conversion.

		bool GetFilePath(std::wstring *pstrFilePath);

		// The stream will not be AddRef'd by this call. It will exist as long as the CFileAndSteam does
		// so you should not normally need to AddRef it.
		// If a stream is returned it will always be fast and seekable.
		// Whenever GetStream_NoRef_Conversion_SeekReset is called the stream's position is reset to the start.
		bool GetStream_NoRef_Conversion_SeekReset(IStream **ppStream, bool *pOutNoRandomSeek, bool *pOutSlow, bool *pOutSlowSeek);

		// Call this to get the current IStream, if any.
		// Unlike the GetStream_NoRef_Conversion_SeekReset() method:
		// - The returned IStream *will* be AddRef'd and you must Release it.
		// - The returned IStream may not be seekable.
		// - If the object contains a filepath that hasn't already been converted into an IStream then that conversion will not happen and the method will fail instead.
		// - The returned IStream will not have its position reset to the start of the IStream (since it may not be seekable).
		bool GetStream_AddRef_NoConversion_NoSeekReset(IStream **ppStream, bool *pOutNoRandomSeek, bool *pOutSlow, bool *pOutSlowSeek);

	private:
		bool PrepareFileForReading();
		bool WriteStreamToTempFile();

	private:
		std::wstring m_strFileName;

		std::wstring m_strFilePath;
		bool m_bDeleteFilePathOnClose;
		bool m_bOpenTempCopyFilePath;

		IStream *m_pStream;
		bool m_bNoRandomSeekInStream;
		bool m_bSlowStream;
		bool m_bSlowSeekStream;

		bool m_bStreamIsFile;

		HANDLE m_hAbortEvent;
	};

	class DirEnum
	{
	private:
		WIN32_FIND_DATA m_wfd;
		HANDLE m_hFind;
		std::wstring m_strDirPath;

		DirEnum(const DirEnum &rhs); // disallow
		DirEnum &operator=(const DirEnum &rhs); // disallow

		bool skipDots();

	public:
		DirEnum() : m_wfd() , m_hFind(INVALID_HANDLE_VALUE) { }
		~DirEnum() { Close(); } // Not virtual.
		bool Init(DWORD *pdwError, const wchar_t *szDirPath, const wchar_t *szPattern);
		void Close();
		bool AtEnd() const { return (m_hFind == INVALID_HANDLE_VALUE); };
		const WIN32_FIND_DATA &CurrentData() const { assert(!AtEnd()); return m_wfd; }
		void CurrentFullPath(std::wstring *pStr) const;
		bool Step(DWORD *pdwError);
	};
};
