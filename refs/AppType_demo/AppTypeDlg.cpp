/////////////////////////////////////////////////////////////////////////////
// AppTypeDlg.cpp : Implementation of Dialog based application
//
// A Dialog Based Application
//
// All rights reserved.
//
// Written by Naveen Kohli (naveen@a3ds.com)
// Version 1.0
//
// Distribute freely, except: don't remove my name from the source or
// documentation (don't take credit for my work), mark your changes (don't
// get me blamed for your possible bugs), don't alter or remove this
// notice.
// No warrantee of any kind, express or implied, is included with this
// software; use at your own risk, responsibility for damages (if any) to
// anyone resulting from the use of this software rests entirely with the
// user.
//
// Send bug reports, bug fixes, enhancements, requests, flames, etc. to
//    naveen@a3ds.com
/////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "AppType.h"
#include "AppTypeDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CAppTypeDlg dialog

CAppTypeDlg::CAppTypeDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CAppTypeDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CAppTypeDlg)
	m_stFilePath = _T("");
	m_stAppType = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CAppTypeDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAppTypeDlg)
	DDX_Control(pDX, IDC_APPTYPE_STATIC, m_AppTypeStatic);
	DDX_Control(pDX, IDC_FILEPATH_EDIT, m_FilePathEdit);
	DDX_Text(pDX, IDC_FILEPATH_EDIT, m_stFilePath);
	DDX_Text(pDX, IDC_APPTYPE_STATIC, m_stAppType);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAppTypeDlg, CDialog)
	//{{AFX_MSG_MAP(CAppTypeDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_EN_UPDATE(IDC_FILEPATH_EDIT, OnUpdateFilepathEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CAppTypeDlg message handlers

BOOL CAppTypeDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// TODO: Add extra initialization here
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CAppTypeDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CAppTypeDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CAppTypeDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CAppTypeDlg::OnOK() 
{
	HANDLE hImage;
	DWORD dwCoffHeaderOffset;
	DWORD dwNewOffset;
	DWORD dwMoreDosHeader[16];
	ULONG ulNTSignature;

	IMAGE_DOS_HEADER dos_header;
	IMAGE_FILE_HEADER file_header;
	IMAGE_OPTIONAL_HEADER optional_header;

	UpdateData (TRUE);

	// Open the application file.

	hImage = CreateFile (m_stFilePath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL, NULL);

	if (hImage == INVALID_HANDLE_VALUE) {
		AfxMessageBox (_T ("Failed to open the aplication file!\n"));
		return;
	}

	// Read MS-Dos image header.

	if (!ReadFileBytes (hImage, &dos_header, sizeof (IMAGE_DOS_HEADER))) {
		AfxMessageBox (_T ("Failed to read file!\n"));
		return;
	}
	
	if (dos_header.e_magic != IMAGE_DOS_SIGNATURE) {
		AfxMessageBox (_T ("Application failed to classify the file type!\n"));
		return;
	}

	// Read more MS-Dos header.

	if (!ReadFileBytes (hImage, dwMoreDosHeader, sizeof (dwMoreDosHeader))) {
		AfxMessageBox (_T ("Failed to read file!\n"));
		return;
	}

	// Move the file pointer to get the actual COFF header.

	dwNewOffset = SetFilePointer (hImage, dos_header.e_lfanew, NULL, FILE_BEGIN);
	dwCoffHeaderOffset = dwNewOffset + sizeof (ULONG);

	if (dwCoffHeaderOffset == 0xFFFFFFFF) {
		AfxMessageBox (_T ("Failed to move file pointer!\n"));
		return;
	}

	// Read NT signature of the file.

	if (!ReadFileBytes (hImage, &ulNTSignature, sizeof (ULONG))) {
		AfxMessageBox (_T ("Failed to read NT signature of file!\n"));
		return;
	}

	if (ulNTSignature != IMAGE_NT_SIGNATURE) {
		AfxMessageBox (_T ("Missing NT signature!\n"));
		return;
	}

	if (!ReadFileBytes (hImage, &file_header, IMAGE_SIZEOF_FILE_HEADER)) {
		AfxMessageBox (_T ("Failed to read file!\n"));
		return;
	}

	// Read the optional header of file.

	if (!ReadFileBytes (hImage, &optional_header, IMAGE_SIZEOF_NT_OPTIONAL_HEADER)) {
		AfxMessageBox (_T ("Failed to read file for optional header!\n"));
		return;
	}

	switch (optional_header.Subsystem) {
		case IMAGE_SUBSYSTEM_UNKNOWN:
			m_stAppType = _T ("Unknown");
			break;
		case IMAGE_SUBSYSTEM_NATIVE:
			m_stAppType = _T ("Native");
			break;
		case IMAGE_SUBSYSTEM_WINDOWS_GUI:
			m_stAppType = _T ("Windows GUI");
			break;
		case IMAGE_SUBSYSTEM_WINDOWS_CUI:
			m_stAppType = _T ("Windows Console");
			break;
		case IMAGE_SUBSYSTEM_OS2_CUI:
			m_stAppType = _T ("OS//2 Console");
			break;
		case IMAGE_SUBSYSTEM_POSIX_CUI:
			m_stAppType = _T ("Posix Console");
			break;
		case IMAGE_SUBSYSTEM_NATIVE_WINDOWS:
			m_stAppType = _T ("Native Win9x");
			break;
		case IMAGE_SUBSYSTEM_WINDOWS_CE_GUI:
			m_stAppType = _T ("Windows CE GUI");
			break;
	}

	UpdateData (FALSE);
}

void CAppTypeDlg::OnUpdateFilepathEdit() 
{
	// TODO: If this is a RICHEDIT control, the control will not
	// send this notification unless you override the CDialog::OnInitDialog()
	// function to send the EM_SETEVENTMASK message to the control
	// with the ENM_UPDATE flag ORed into the lParam mask.
	
	// TODO: Add your control notification handler code here
	
}

bool CAppTypeDlg::ReadFileBytes(HANDLE hFile, LPVOID lpBuffer, DWORD dwSize)
{
	DWORD dwBytes = 0;

	if (!ReadFile (hFile, lpBuffer, dwSize, &dwBytes, NULL)) {
		TRACE (_T ("Failed to read file!\n"));
		return (false);
	}

	if (dwSize != dwBytes) {
		TRACE (_T ("Wrong number of bytes read, expected\n"), dwSize, dwBytes);
		return (false);
	}

	return (true);
}
