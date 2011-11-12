#include "stdafx.h"

#include <windows.h>
#include <winnt.h>
#include "WinApiException.h"
#include "Payload.Dll.h"

//void IsConsoleApp(TCHAR* programPath);
DWORD AbsoluteSeek( HANDLE, DWORD );
void ReadFileToBuffer( HANDLE, LPVOID, DWORD );
void WriteBytes( HANDLE, LPVOID, DWORD );
void CopySection( HANDLE, HANDLE, DWORD );

#define IMAGE_SIZEOF_NT_OPTIONAL_HEADER sizeof(IMAGE_OPTIONAL_HEADER)

void ReadFileToBuffer( HANDLE file, LPVOID buffer, DWORD bufferSize )
{
    DWORD bytesRead;

    BOOL success = ReadFile(file, buffer, bufferSize, &bytesRead, NULL);
    if( !success ) { throw WinApiException("ReadFile"); }
    if( bufferSize != bytesRead ) { throw WinApiException("ReadFile: unexpected number of bytes were read"); }    
}

HOOKDLL_API void APIENTRY IsConsoleApp( TCHAR* programPath )
{       
    HANDLE file = CreateFile( programPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
    if( INVALID_HANDLE_VALUE == file ) { throw WinApiException("CreateFile"); }

    IMAGE_DOS_HEADER image_dos_header;
    ReadFileToBuffer( file, &image_dos_header, sizeof(image_dos_header) ); 
    if( IMAGE_DOS_SIGNATURE != image_dos_header.e_magic ) { throw WinApiException("ReadFileToBuffer: file image signature is unknown"); }

    //DWORD more_dos_header[16];
    //ReadFileToBuffer( file, more_dos_header, sizeof(more_dos_header) );

    // actual COFF header
    DWORD coffHeaderOffset = SetFilePointer( file, image_dos_header.e_lfanew, NULL, FILE_BEGIN );
    if( 0xFFFFFFFF == coffHeaderOffset ) { throw WinApiException("SetFilePointer"); } 

    ULONG ntSignature;
    ReadFileToBuffer( file, &ntSignature, sizeof(ntSignature) );
    if( IMAGE_NT_SIGNATURE != ntSignature ) { throw WinApiException("ReadFileToBuffer: missing NT signature"); }

    IMAGE_FILE_HEADER image_file_header;
    DWORD test = sizeof(IMAGE_FILE_HEADER);
    ReadFileToBuffer(file, &image_file_header, IMAGE_SIZEOF_FILE_HEADER);

    IMAGE_OPTIONAL_HEADER image_optional_header;
    ReadFileToBuffer(file, &image_optional_header, IMAGE_SIZEOF_NT_OPTIONAL_HEADER);

    switch( image_optional_header.Subsystem )
    {
    case IMAGE_SUBSYSTEM_UNKNOWN:
        printf("Type is unknown.\n");
        break;

    case IMAGE_SUBSYSTEM_NATIVE:
        printf("Type is native.\n");
        break;

    case IMAGE_SUBSYSTEM_WINDOWS_GUI:
        printf("Type is Windows GUI.\n");
        break;

    case IMAGE_SUBSYSTEM_WINDOWS_CUI:
        printf("Type is Windows CUI.\n");
        break;

    case IMAGE_SUBSYSTEM_OS2_CUI:
        printf("Type is OS/2 CUI.\n");
        break;

    case IMAGE_SUBSYSTEM_POSIX_CUI:
        printf("Type is POSIX CUI.\n");
        break;

    case IMAGE_SUBSYSTEM_NATIVE_WINDOWS:
           printf("Type is native Win9x driver.\n");
           break;

    case IMAGE_SUBSYSTEM_WINDOWS_CE_GUI:
        printf("Type is Windows CE.\n");
        break;

    default:
        printf("Unknown type %u.\n", image_optional_header.Subsystem);
        break;
    }
}


