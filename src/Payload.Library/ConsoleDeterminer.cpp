#include "stdafx.h"

#include <windows.h>
#include <winnt.h>
#include "WinApiException.h"
#include "Payload.Dll.h"

void ReadFileToBuffer( HANDLE file, LPVOID buffer, DWORD bufferSize )
{
    DWORD bytesRead;

    BOOL success = ReadFile(file, buffer, bufferSize, &bytesRead, NULL);
    if( !success ) { throw WinApiException("ReadFile"); }
    if( bufferSize != bytesRead ) { throw WinApiException("ReadFile: unexpected number of bytes were read"); }    
}

HOOKDLL_API bool APIENTRY IsConsoleApp( TCHAR* programPath )
{       
    // Open file needed to be verified
    HANDLE file = CreateFile( programPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
    if( INVALID_HANDLE_VALUE == file ) { throw WinApiException("CreateFile"); }

    // Verify DOS compability signature (MZ)
    IMAGE_DOS_HEADER image_dos_header;
    ReadFileToBuffer( file, &image_dos_header, sizeof(image_dos_header) ); 
    if( IMAGE_DOS_SIGNATURE != image_dos_header.e_magic ) { throw WinApiException("ReadFileToBuffer: file image signature is unknown"); }

    // Move file pointer to actual COFF header
    DWORD coffHeaderOffset = SetFilePointer( file, image_dos_header.e_lfanew, NULL, FILE_BEGIN );
    if( INVALID_SET_FILE_POINTER == coffHeaderOffset ) { throw WinApiException("SetFilePointer"); } 

    // Verify NT signature correctness (PE00)
    ULONG ntSignature;
    ReadFileToBuffer( file, &ntSignature, sizeof(ntSignature) );
    if( IMAGE_NT_SIGNATURE != ntSignature ) { throw WinApiException("ReadFileToBuffer: missing NT signature"); }

    // Read file header. File pointer is moved forward.
    IMAGE_FILE_HEADER image_file_header;
    ReadFileToBuffer(file, &image_file_header, sizeof(IMAGE_FILE_HEADER));

    // Read file optional header that is located after image file header
    IMAGE_OPTIONAL_HEADER image_optional_header;
    ReadFileToBuffer(file, &image_optional_header, sizeof(IMAGE_OPTIONAL_HEADER));

    // Return true if file has Console subsystem
    return IMAGE_SUBSYSTEM_WINDOWS_CUI == image_optional_header.Subsystem;
}


