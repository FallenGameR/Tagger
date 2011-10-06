#include "stdafx.h"

// Global Variables
TCHAR* szTitle = TEXT("Target");
TCHAR* szWindowClass = TEXT("TARGET");
HFONT defaultFont;
RECT currentRect, moveRect, movingRect, sizeRect, sizingRect;

// Forwards
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam );

int APIENTRY _tWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
{
    // Register window class
    WNDCLASSEX wcex; ZeroMemory( &wcex, sizeof(WNDCLASSEX) );
    wcex.cbSize         = sizeof(WNDCLASSEX);
    wcex.style		    = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc	= WndProc;
    wcex.hInstance		= hInstance;
    wcex.hIcon			= LoadIcon( NULL, IDI_INFORMATION );
    wcex.hCursor		= LoadCursor( NULL, IDC_ARROW );
    wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszClassName	= szWindowClass;
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
    LOGFONT font; ZeroMemory( &font, sizeof(LOGFONT) );

    font.lfHeight = -24;	
    _tcscpy( font.lfFaceName, TEXT("Segoe UI Semibold") );

    return CreateFontIndirect( &font );
}

void RenderRect( TCHAR buffer[256], RECT rect )
{
    LONG x = rect.left;
    LONG y = rect.top;
    LONG width = rect.right - rect.left;
    LONG height = rect.bottom - rect.top;
    
    _stprintf( buffer, TEXT( "x %d y %d w %d h %d" ), x, y, width, height );
}

void RenderReport( TCHAR text[4096] )
{
    TCHAR current[256], move[256], moving[256], size[256], sizing[256];

    RenderRect( current, currentRect );
    RenderRect( move, moveRect );
    RenderRect( moving, movingRect );
    RenderRect( size, sizeRect );
    RenderRect( sizing, sizingRect );

    _stprintf( text, TEXT( "Current\t%s\nMove\t%s\nMoving\t%s\nSize\t%s\nSizing\t%s" ), current, move, moving, size, sizing );
}

LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    TCHAR text[ 4096 ];
    size_t textLength;
    PAINTSTRUCT paint;
    HDC hdc;  

    switch( message )
    {
    case WM_CREATE:
        defaultFont = CreateFont();
        SetWindowPos( hWnd, NULL, 400, 600, 900, 400, 0 );

        GetWindowRect( hWnd, &currentRect );
        moveRect = movingRect = sizeRect = sizingRect = currentRect;
        break;

    case WM_DESTROY:
        DeleteObject( defaultFont );
        PostQuitMessage( 0 );
        break;

    case WM_PAINT:
        hdc = BeginPaint( hWnd, &paint );
        SelectObject( hdc, defaultFont );

        GetWindowRect( hWnd, &currentRect );
        RenderReport( text );
        textLength = _tcslen( text );
        DrawText( paint.hdc, text, (int) textLength, &paint.rcPaint, DT_EXPANDTABS );

        EndPaint( hWnd, &paint );
        break;

    case WM_MOVE:
        GetWindowRect( hWnd, &moveRect );
        InvalidateRect( hWnd, NULL, FALSE );
        break;

    case WM_MOVING:
        GetWindowRect( hWnd, &movingRect );
        break;

    case WM_SIZE:
        GetWindowRect( hWnd, &sizeRect );
        break;

    case WM_SIZING:
        GetWindowRect( hWnd, &sizingRect );
        break;
    }

    return DefWindowProc( hWnd, message, wParam, lParam );
}
