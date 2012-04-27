//-----------------------------------------------------------------------
// <copyright file="PortableExecutable.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.WinAPI
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Structures that define PE header
    /// </summary>
    public static partial class NativeMethods
    {
        /// <summary>
        /// Flag in FileHeader.Characteristics for computer that supports 32-bit words
        /// </summary>
        public const ushort IMAGE_FILE_32BIT_MACHINE = 0x0100;

        #region IMAGE_SUBSYSTEM_WINDOWS enum 

        /// <summary>
        /// Windows subsystems enumeration values
        /// </summary>
        public enum IMAGE_SUBSYSTEM_WINDOWS : ushort
        {
            /// <summary>
            /// Console subsystem
            /// </summary>
            CUI = 3,
        }

        #endregion

        #region IMAGE_FILE_HEADER struct

        /// <summary>
        /// File header that precedes optional PE header
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        #endregion

        #region IMAGE_DOS_HEADER struct

        /// <summary>
        /// DOS header from PE
        /// </summary>
        public struct IMAGE_DOS_HEADER
        {
            public ushort e_magic;              // Magic number
            public ushort e_cblp;               // Bytes on last page of file
            public ushort e_cp;                 // Pages in file
            public ushort e_crlc;               // Relocations
            public ushort e_cparhdr;            // Size of header in paragraphs
            public ushort e_minalloc;           // Minimum extra paragraphs needed
            public ushort e_maxalloc;           // Maximum extra paragraphs needed
            public ushort e_ss;                 // Initial (relative) SS value
            public ushort e_sp;                 // Initial SP value
            public ushort e_csum;               // Checksum
            public ushort e_ip;                 // Initial IP value
            public ushort e_cs;                 // Initial (relative) CS value
            public ushort e_lfarlc;             // File address of relocation table
            public ushort e_ovno;               // Overlay number
            public ushort e_res_0;              // Reserved words
            public ushort e_res_1;              // Reserved words
            public ushort e_res_2;              // Reserved words
            public ushort e_res_3;              // Reserved words
            public ushort e_oemid;              // OEM identifier (for e_oeminfo)
            public ushort e_oeminfo;            // OEM information; e_oemid specific
            public ushort e_res2_0;             // Reserved words
            public ushort e_res2_1;             // Reserved words
            public ushort e_res2_2;             // Reserved words
            public ushort e_res2_3;             // Reserved words
            public ushort e_res2_4;             // Reserved words
            public ushort e_res2_5;             // Reserved words
            public ushort e_res2_6;             // Reserved words
            public ushort e_res2_7;             // Reserved words
            public ushort e_res2_8;             // Reserved words
            public ushort e_res2_9;             // Reserved words
            public uint e_lfanew;             // File address of new exe header
        }

        #endregion

        #region IMAGE_OPTIONAL_HEADER32 struct

        /// <summary>
        /// Optional PE header for x86
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;
            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
        }

        #endregion

        #region IMAGE_OPTIONAL_HEADER64 struct
        
        /// <summary>
        /// Optional PE header for x64
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public ulong ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public ulong SizeOfStackReserve;
            public ulong SizeOfStackCommit;
            public ulong SizeOfHeapReserve;
            public ulong SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
        }
 
        #endregion    
    }
}
