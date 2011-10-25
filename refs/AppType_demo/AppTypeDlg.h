// AppTypeDlg.h : header file
//

#if !defined(AFX_APPTYPEDLG_H__5BC785F3_D985_11D2_8C99_000000000000__INCLUDED_)
#define AFX_APPTYPEDLG_H__5BC785F3_D985_11D2_8C99_000000000000__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CAppTypeDlg dialog

class CAppTypeDlg : public CDialog
{
// Construction
public:
	CAppTypeDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CAppTypeDlg)
	enum { IDD = IDD_APPTYPE_DIALOG };
	CStatic	m_AppTypeStatic;
	CEdit	m_FilePathEdit;
	CString	m_stFilePath;
	CString	m_stAppType;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAppTypeDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CAppTypeDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	virtual void OnOK();
	afx_msg void OnUpdateFilepathEdit();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
private:
	bool ReadFileBytes (HANDLE hFile, LPVOID lpBuffer, DWORD dwSize);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_APPTYPEDLG_H__5BC785F3_D985_11D2_8C99_000000000000__INCLUDED_)
