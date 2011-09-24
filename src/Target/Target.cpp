#include "stdafx.h"
#include "Target.h"

// Global Variables
TCHAR* szTitle = TEXT("Target");
TCHAR* szWindowClass = TEXT("TARGET");
HFONT defaultFont;

int APIENTRY _tWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
{
    // Register window class
    WNDCLASSEX wcex;    
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.style			= CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc	= WndProc;
    wcex.cbClsExtra		= 0;
    wcex.cbWndExtra		= 0;
    wcex.hInstance		= hInstance;
    wcex.hIcon			= LoadIcon( hInstance, MAKEINTRESOURCE(IDI_TARGET) );
    wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName	= 0;
    wcex.lpszClassName	= szWindowClass;
    wcex.hIconSm		= LoadIcon( wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL) );
    RegisterClassEx( &wcex );

    // Initialize window
    HWND hWnd = CreateWindow( szWindowClass, szTitle, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, NULL, NULL, hInstance, NULL );
    if( !hWnd ) { return FALSE; }

    ShowWindow( hWnd, nCmdShow );
    UpdateWindow( hWnd );

    // Main message loop
    MSG msg;

    while( GetMessage( &msg, NULL, 0, 0 ) )
    {
        TranslateMessage( &msg );
        DispatchMessage( &msg );
    }

    return (int) msg.wParam;
}



HFONT CreateFont()
{
    LOGFONT font;

    font.lfHeight = -24;	
    font.lfEscapement = 0;
    font.lfItalic = 0;
    font.lfUnderline = 0;
    font.lfStrikeOut = 0;
    _tcscpy( font.lfFaceName, TEXT("Segoe UI Semibold") );

    return CreateFontIndirect( &font );
}

void RenderRect( TCHAR* buffer, RECT rect )
{
    LONG x = rect.left;
    LONG y = rect.top;
    LONG width = rect.right - rect.left;
    LONG height = rect.bottom - rect.top;
    
    _stprintf( buffer, TEXT( "x %d y %d w %d h %d" ), x, y, width, height );
}


LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    PAINTSTRUCT paint;
    HDC hdc;
    int xPos, yPos, width, height;
    RECT position;
    LPCTSTR str = TEXT("Hello World");


    

    RECT defaultRect;
    RECT currentRect;
    GetWindowRect( hWnd, &currentRect ); 

    TCHAR current[256], move[256], moving[256], size[256], sizing[256], result[ 4096 ];
    RenderRect( current, currentRect );

    _stprintf( result, TEXT( "Current\t%s\nMove\t%s\nMoving\t%s\nSize\t%s\nSizing\t%s" ), current, move, moving, size, sizing );
    str = result;	
    int len = (int)_tcslen(str);



    switch( message )
    {
    case WM_CREATE:
        defaultFont = CreateFont();
        break;

    case WM_DESTROY:
        DeleteObject( defaultFont );
        PostQuitMessage( 0 );
        break;

    case WM_PAINT:
        hdc = BeginPaint( hWnd, &paint );
        SelectObject( hdc, defaultFont );
        DrawText( paint.hdc, str, len, &paint.rcPaint, DT_EXPANDTABS );
        EndPaint( hWnd, &paint );
        break;

    case WM_MOVE:
        xPos = (int)(short) LOWORD(lParam);
        yPos = (int)(short) HIWORD(lParam);
        break;

    case WM_SIZE:
        width = (int)(short) LOWORD(lParam);
        height = (int)(short) HIWORD(lParam);
        break;

    case WM_MOVING:
        position = * (RECT*) lParam;
        break;

    case WM_SIZING:
        position = * (RECT*) lParam;
        break;
    }

    return DefWindowProc(hWnd, message, wParam, lParam);
}
