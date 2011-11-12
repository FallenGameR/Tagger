#pragma once
#include "stdafx.h"

void GrantDebugPrivilege();
bool IsConhost( DWORD pid );
DWORD FindParentConhost( HWCT wctSession, DWORD tid );