// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"
#include <windows.h>
#include <shlobj.h>

//#include <stdlib.h>
#include <assert.h>
#include <process.h>

#include <string>
#include <vector>
#include <deque>
#include <map>
#include <set>
#include <iterator>
#include <algorithm>

bool NoBarTabConfig_Load(std::vector< std::wstring > &vecExes, bool bDefaultIfNotInRegistry);
bool NoBarTabConfig_Save(const std::vector< std::wstring > &vecExes);
bool NoBarTabConfig_Install(const wchar_t *szCmd);
bool NoBarTabConfig_IsInstalled(const wchar_t *szCmd);
bool NoBarTabConfig_Wipe();
