#include "stdafx.h"
#include "Target.h"

// Global Variables
TCHAR* szTitle = TEXT("Target");
TCHAR* szWindowClass = TEXT("TARGET");
HFONT defaultFont;
RECT currentRect, moveRect, movingRect, sizeRect, sizingRect;


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
        moveRect.left = LOWORD(lParam);
        moveRect.top = HIWORD(lParam);
        InvalidateRect( hWnd, NULL, FALSE );
        break;

    case WM_SIZE:
        sizeRect.right = sizeRect.left + LOWORD(lParam);
        sizeRect.bottom = sizeRect.top + HIWORD(lParam);
        break;

    case WM_MOVING:
        movingRect = * (RECT*) lParam;
        InvalidateRect( hWnd, NULL, FALSE );
        break;

    case WM_SIZING:
        sizingRect = * (RECT*) lParam;
        break;
    }

    return DefWindowProc(hWnd, message, wParam, lParam);
}
