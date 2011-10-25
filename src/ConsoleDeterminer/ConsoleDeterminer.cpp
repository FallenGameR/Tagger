#include "stdafx.h"
#include <windows.h>
#include <winnt.h>

VOID  main(int, char **);
DWORD AbsoluteSeek(HANDLE, DWORD);
VOID  ReadBytes(HANDLE, LPVOID, DWORD);
VOID  WriteBytes(HANDLE, LPVOID, DWORD);
VOID  CopySection(HANDLE, HANDLE, DWORD);

// http://support.microsoft.com/kb/90493
#define XFER_BUFFER_SIZE 2048

// http://blogs.msdn.com/b/jeremykuhne/archive/2008/02/19/sscli-2-0-and-visual-studio-2008.aspx
#pragma warning (disable :4985)

#define IMAGE_SIZEOF_NT_OPTIONAL32_HEADER    224
#define IMAGE_SIZEOF_NT_OPTIONAL64_HEADER    240

//sizeof(IMAGE_OPTIONAL_HEADER)
#ifdef _WIN64
#define IMAGE_SIZEOF_NT_OPTIONAL_HEADER     IMAGE_SIZEOF_NT_OPTIONAL64_HEADER
#else
#define IMAGE_SIZEOF_NT_OPTIONAL_HEADER     IMAGE_SIZEOF_NT_OPTIONAL32_HEADER
#endif

VOID
main(int argc, char *argv[])
{   
    HANDLE hImage;

    DWORD  bytes;
    DWORD  iSection;
    DWORD  SectionOffset;
    DWORD  CoffHeaderOffset;
    DWORD  MoreDosHeader[16];

    ULONG  ntSignature;

    IMAGE_DOS_HEADER      image_dos_header;
    IMAGE_FILE_HEADER     image_file_header;
    IMAGE_OPTIONAL_HEADER image_optional_header;
    IMAGE_SECTION_HEADER  image_section_header;

    if (argc != 2)
    {
        printf("USAGE: %s program_file_name\n", argv[1]);
        exit(1);
    }

    /*
     *  Open the reference file.
     */ 
    hImage = CreateFile(argv[1],
                        GENERIC_READ,
                        FILE_SHARE_READ,
                        NULL,
                        OPEN_EXISTING,
                        FILE_ATTRIBUTE_NORMAL,
                        NULL);

    if (INVALID_HANDLE_VALUE == hImage)
    {
        printf("Could not open %s, error %lu\n", argv[1], GetLastError());
        exit(1);
    }

    /*
     *  Read the MS-DOS image header.
     */ 
    ReadBytes(hImage,
              &image_dos_header,
              sizeof(IMAGE_DOS_HEADER));

    if (IMAGE_DOS_SIGNATURE != image_dos_header.e_magic)
    {
        printf("Sorry, I do not understand this file.\n");
        exit(1);
    }

    /*
     *  Read more MS-DOS header.       */ 
    ReadBytes(hImage,
              MoreDosHeader,
              sizeof(MoreDosHeader));

    /*
     *  Get actual COFF header.
     */ 
    CoffHeaderOffset = AbsoluteSeek(hImage, image_dos_header.e_lfanew) +
                       sizeof(ULONG);

    ReadBytes (hImage, &ntSignature, sizeof(ULONG));

    if (IMAGE_NT_SIGNATURE != ntSignature)
    {
     printf("Missing NT signature. Unknown file type.\n");
     exit(1);
    }

    SectionOffset = CoffHeaderOffset + IMAGE_SIZEOF_FILE_HEADER +
                    IMAGE_SIZEOF_NT_OPTIONAL_HEADER;

    ReadBytes(hImage,
              &image_file_header,
              IMAGE_SIZEOF_FILE_HEADER);

    /*
     *  Read optional header.
     */ 
    ReadBytes(hImage,
              &image_optional_header,
              IMAGE_SIZEOF_NT_OPTIONAL_HEADER);

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

DWORD
AbsoluteSeek(HANDLE hFile,
             DWORD  offset)
{
    DWORD newOffset;

    if ((newOffset = SetFilePointer(hFile,
                                    offset,
                                    NULL,
                                    FILE_BEGIN)) == 0xFFFFFFFF)
    {
        printf("SetFilePointer failed, error %lu.\n", GetLastError());
        exit(1);
    }

    return newOffset;
}

VOID
ReadBytes(HANDLE hFile,
          LPVOID buffer,
          DWORD  size)
{
    DWORD bytes;

    if (!ReadFile(hFile,
                  buffer,
                  size,
                  &bytes,
                  NULL))
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