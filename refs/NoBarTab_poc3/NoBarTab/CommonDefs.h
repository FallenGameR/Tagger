#pragma once

// These definitions are shared with both the Exe and Dll.

extern const wchar_t * const g_szMainWindowClass64;
extern const wchar_t * const g_szMainWindowClass32;
extern const wchar_t * const g_szMainWindowClass;

#define WM_NOBARTAB_WINDOWCREATED_CBT	(WM_USER + 1)
#define WM_NOBARTAB_STOP				(WM_USER + 2)
#define WM_NOBARTAB_SHOWCONFIG			(WM_USER + 3)
#define WM_NOBARTAB_REFRESHCONFIG		(WM_USER + 4)
