// AppType.h : main header file for the APPTYPE application
//

#if !defined(AFX_APPTYPE_H__5BC785F1_D985_11D2_8C99_000000000000__INCLUDED_)
#define AFX_APPTYPE_H__5BC785F1_D985_11D2_8C99_000000000000__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CAppTypeApp:
// See AppType.cpp for the implementation of this class
//

class CAppTypeApp : public CWinApp
{
public:
	CAppTypeApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAppTypeApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CAppTypeApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_APPTYPE_H__5BC785F1_D985_11D2_8C99_000000000000__INCLUDED_)
