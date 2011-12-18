﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

namespace Tagger
{
    /// <summary>
    /// Reads in the header information of the Portable Executable format.
    /// Provides information such as the date the assembly was compiled. 
    /// Original from http://code.cheesydesign.com/?p=572
    /// </summary>
    public class PEParser
    {
        #region File Header Structures

        // DOS .EXE header
        public struct IMAGE_DOS_HEADER
        {      
            public UInt16 e_magic;              // Magic number
            public UInt16 e_cblp;               // Bytes on last page of file
            public UInt16 e_cp;                 // Pages in file
            public UInt16 e_crlc;               // Relocations
            public UInt16 e_cparhdr;            // Size of header in paragraphs
            public UInt16 e_minalloc;           // Minimum extra paragraphs needed
            public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
            public UInt16 e_ss;                 // Initial (relative) SS value
            public UInt16 e_sp;                 // Initial SP value
            public UInt16 e_csum;               // Checksum
            public UInt16 e_ip;                 // Initial IP value
            public UInt16 e_cs;                 // Initial (relative) CS value
            public UInt16 e_lfarlc;             // File address of relocation table
            public UInt16 e_ovno;               // Overlay number
            public UInt16 e_res_0;              // Reserved words
            public UInt16 e_res_1;              // Reserved words
            public UInt16 e_res_2;              // Reserved words
            public UInt16 e_res_3;              // Reserved words
            public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo;            // OEM information; e_oemid specific
            public UInt16 e_res2_0;             // Reserved words
            public UInt16 e_res2_1;             // Reserved words
            public UInt16 e_res2_2;             // Reserved words
            public UInt16 e_res2_3;             // Reserved words
            public UInt16 e_res2_4;             // Reserved words
            public UInt16 e_res2_5;             // Reserved words
            public UInt16 e_res2_6;             // Reserved words
            public UInt16 e_res2_7;             // Reserved words
            public UInt16 e_res2_8;             // Reserved words
            public UInt16 e_res2_9;             // Reserved words
            public UInt32 e_lfanew;             // File address of new exe header
        }

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt64 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt64 SizeOfStackReserve;
            public UInt64 SizeOfStackCommit;
            public UInt64 SizeOfHeapReserve;
            public UInt64 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
        }

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        #endregion File Header Structures

        #region Private Fields

        // The DOS header
        private IMAGE_DOS_HEADER dosHeader;
        // The file header
        private IMAGE_FILE_HEADER fileHeader;
        // Optional 32 bit file header
        private IMAGE_OPTIONAL_HEADER32 optionalHeader32;
        // Optional 64 bit file header
        private IMAGE_OPTIONAL_HEADER64 optionalHeader64;

        #endregion Private Fields

        #region Public Methods

        public PEParser( string filePath )
        {
            // Read in the DLL or EXE and get the timestamp
            using( FileStream stream = new FileStream( filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read ) )
            {
                BinaryReader reader = new BinaryReader( stream );
                dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>( reader );

                // Add 4 bytes to the offset
                stream.Seek( dosHeader.e_lfanew, SeekOrigin.Begin );

                UInt32 ntHeadersSignature = reader.ReadUInt32();
                fileHeader = FromBinaryReader<IMAGE_FILE_HEADER>( reader );
                if( this.Is32BitHeader )
                {
                    optionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>( reader );
                }
                else
                {
                    optionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>( reader );
                }
            }
        }

        // Gets the header of the .NET assembly that called this function
        public static PEParser GetCallingAssemblyHeader()
        {
            string pathCallingAssembly = System.Reflection.Assembly.GetCallingAssembly().Location;

            // Get the path to the calling assembly, which is the path to the
            // DLL or EXE that we want the time of
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;

            // Get and return the timestamp
            return new PEParser( filePath );
        }

        // Reads in a block from a file and converts it to the struct
        // type specified by the template parameter
        public static T FromBinaryReader<T>( BinaryReader reader )
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes( Marshal.SizeOf( typeof( T ) ) );

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            T theStructure = (T) Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
            handle.Free();

            return theStructure;
        }

        #endregion Public Methods

        #region Properties

        // Gets if the file header is 32 bit or not
        public bool Is32BitHeader
        {
            get
            {
                UInt16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
                return (IMAGE_FILE_32BIT_MACHINE & FileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
            }
        }

        // Gets the file header
        public IMAGE_FILE_HEADER FileHeader
        {
            get
            {
                return fileHeader;
            }
        }

        // Gets the optional header
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader32
        {
            get
            {
                return optionalHeader32;
            }
        }

        // Gets the optional header
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader64
        {
            get
            {
                return optionalHeader64;
            }
        }

        // Gets the timestamp from the file header
        public DateTime TimeStamp
        {
            get
            {
                // Timestamp is a date offset from 1970
                DateTime returnValue = new DateTime( 1970, 1, 1, 0, 0, 0 );

                // Add in the number of seconds since 1970/1/1
                returnValue = returnValue.AddSeconds( fileHeader.TimeDateStamp );
                // Adjust to local timezone
                returnValue += TimeZone.CurrentTimeZone.GetUtcOffset( returnValue );

                return returnValue;
            }
        }

        #endregion Properties
    }
}