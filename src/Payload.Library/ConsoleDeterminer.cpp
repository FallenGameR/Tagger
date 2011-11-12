#include "stdafx.h"
#include <windows.h>
#include <winnt.h>
#include "WinApiException.h"

void IsConsoleApp(TCHAR* programPath);
DWORD AbsoluteSeek(HANDLE, DWORD);
void  ReadBytes(HANDLE, LPVOID, DWORD);
void  WriteBytes(HANDLE, LPVOID, DWORD);
void  CopySection(HANDLE, HANDLE, DWORD);

#define IMAGE_SIZEOF_NT_OPTIONAL_HEADER sizeof(IMAGE_OPTIONAL_HEADER)

void ReadBytes(HANDLE hFile, LPVOID buffer, DWORD  size)
{
    DWORD bytes;

    if (!ReadFile(hFile, buffer, size, &bytes, NULL))
    {
        printf("ReadFile failed, error %lu.\n", GetLastError());
        exit(1);
    }
    else if (size != bytes)
    {
        printf("Read the wrong number of bytes, expected %lu, got %lu.\n",
            size, bytes);
        exit(1);
    }
}


void IsConsoleApp( TCHAR* programPath )
{       
    HANDLE hImage = CreateFile( programPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
    if( INVALID_HANDLE_VALUE == hImage ) { throw WinApiException("CreateFile"); }

    IMAGE_DOS_HEADER image_dos_header;
    ReadBytes( hImage, &image_dos_header, sizeof(IMAGE_DOS_HEADER) ); 
    if( IMAGE_DOS_SIGNATURE != image_dos_header.e_magic ) { throw WinApiException("ReadBytes: file image signature is unknown"); }

    DWORD MoreDosHeader[16];
    ReadBytes( hImage, MoreDosHeader, sizeof(MoreDosHeader) );

    // actual COFF header
    DWORD CoffHeaderOffset = CoffHeaderOffset = AbsoluteSeek(hImage, image_dos_header.e_lfanew) + sizeof(ULONG);

    ULONG ntSignature;
    ReadBytes (hImage, &ntSignature, sizeof(ULONG));

    if (IMAGE_NT_SIGNATURE != ntSignature)
    {
     printf("Missing NT signature. Unknown file type.\n");
     exit(1);
    }

    DWORD SectionOffset = CoffHeaderOffset + IMAGE_SIZEOF_FILE_HEADER + IMAGE_SIZEOF_NT_OPTIONAL_HEADER;

    IMAGE_FILE_HEADER image_file_header;
    ReadBytes(hImage, &image_file_header, IMAGE_SIZEOF_FILE_HEADER);

    /*
     *  Read optional header.
     */ 
    IMAGE_OPTIONAL_HEADER image_optional_header;
    ReadBytes(hImage, &image_optional_header, IMAGE_SIZEOF_NT_OPTIONAL_HEADER);

    switch (image_optional_header.Subsystem)
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

DWORD AbsoluteSeek(HANDLE hFile, DWORD  offset)
{
    DWORD newOffset;

    if ((newOffset = SetFilePointer(hFile, offset, NULL, FILE_BEGIN)) == 0xFFFFFFFF)
    {
        printf("SetFilePointer failed, error %lu.\n", GetLastError());
        exit(1);
    }

    return newOffset;
}

