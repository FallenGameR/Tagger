// Target.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "Target.h"

// Global Variables:
HINSTANCE hInst;								// current instance
TCHAR* szTitle = TEXT("Target");					// The title bar text
TCHAR* szWindowClass = TEXT("TARGET");			// the main window class name

int APIENTRY _tWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
{
    MSG msg;
    HACCEL hAccelTable;

    // Initialize global strings
    MyRegisterClass( hInstance );

    // Perform application initialization:
    if( !InitInstance( hInstance, nCmdShow ) )
    {
        return FALSE;
    }

    hAccelTable = LoadAccelerators( hInstance, MAKEINTRESOURCE(IDC_TARGET) );

    // Main message loop:
    while( GetMessage( &msg, NULL, 0, 0 ) )
    {
        if( !TranslateAccelerator( msg.hwnd, hAccelTable, &msg ) )
        {
            TranslateMessage( &msg );
            DispatchMessage( &msg );
        }
    }

    return (int) msg.wParam;
}


//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
//  COMMENTS:
//
//    This function and its usage are only necessary if you want this code
//    to be compatible with Win32 systems prior to the 'RegisterClassEx'
//    function that was added to Windows 95. It is important to call this function
//    so that the application will get 'well formed' small icons associated
//    with it.
//
ATOM MyRegisterClass( HINSTANCE hInstance )
{
    WNDCLASSEX wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style			= CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc	= WndProc;
    wcex.cbClsExtra		= 0;
    wcex.cbWndExtra		= 0;
    wcex.hInstance		= hInstance;
    wcex.hIcon			= LoadIcon(hInstance, MAKEINTRESOURCE(IDI_TARGET));
    wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName	= MAKEINTRESOURCE(IDC_TARGET);
    wcex.lpszClassName	= szWindowClass;
    wcex.hIconSm		= LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassEx(&wcex);
}


//
//   FUNCTION: InitInstance(HINSTANCE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   HWND hWnd;

   hInst = hInstance; // Store instance handle in our global variable

   hWnd = CreateWindow( szWindowClass, szTitle, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, NULL, NULL, hInstance, NULL );

   if( !hWnd )
   {
      return FALSE;
   }

   ShowWindow( hWnd, nCmdShow );
   UpdateWindow( hWnd );

   return TRUE;
}


//
//  FUNCTION: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    int wmId, wmEvent;
    PAINTSTRUCT ps;
    HDC hdc;

    switch( message )
    {
    case WM_COMMAND:
        wmId    = LOWORD(wParam);
        wmEvent = HIWORD(wParam);

        // Parse the menu selections:
        switch( wmId )
        {
        case IDM_EXIT:
            DestroyWindow( hWnd );
            break;

        default:
            return DefWindowProc( hWnd, message, wParam, lParam );
        }
        break;

    case WM_PAINT:
        hdc = BeginPaint( hWnd, &ps );
        // TODO: Add any drawing code here...
        EndPaint( hWnd, &ps );
        break;

    case WM_DESTROY:
        PostQuitMessage( 0 );
        break;
    }

    return DefWindowProc(hWnd, message, wParam, lParam);
}
