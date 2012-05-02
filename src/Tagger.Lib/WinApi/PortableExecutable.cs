// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortableExecutable.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Tagger.WinAPI
{
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
            /// <summary>
            /// The machine.
            /// </summary>
            public ushort Machine;

            /// <summary>
            /// The number of sections.
            /// </summary>
            public ushort NumberOfSections;

            /// <summary>
            /// The time date stamp.
            /// </summary>
            public uint TimeDateStamp;

            /// <summary>
            /// The pointer to symbol table.
            /// </summary>
            public uint PointerToSymbolTable;

            /// <summary>
            /// The number of symbols.
            /// </summary>
            public uint NumberOfSymbols;

            /// <summary>
            /// The size of optional header.
            /// </summary>
            public ushort SizeOfOptionalHeader;

            /// <summary>
            /// The characteristics.
            /// </summary>
            public ushort Characteristics;
        }

        #endregion

        #region IMAGE_DOS_HEADER struct

        /// <summary>
        /// DOS header from PE
        /// </summary>
        public struct IMAGE_DOS_HEADER
        {
            #region Constants and Fields

            /// <summary>
            /// The e_cblp. Bytes on last page of file.
            /// </summary>
            public ushort e_cblp;

            /// <summary>
            /// The e_cp. Pages in file.
            /// </summary>
            public ushort e_cp;

            /// <summary>
            /// The e_cparhdr. Size of header in paragraphs.
            /// </summary>
            public ushort e_cparhdr;

            /// <summary>
            /// The e_crlc. Relocations.
            /// </summary>
            public ushort e_crlc;

            /// <summary>
            /// The e_cs. Initial (relative) CS value.
            /// </summary>
            public ushort e_cs; 

            /// <summary>
            /// The e_csum. Checksum.
            /// </summary>
            public ushort e_csum;

            /// <summary>
            /// The e_ip. Initial IP value.
            /// </summary>
            public ushort e_ip;

            /// <summary>
            /// The e_lfanew. File address of new exe header.
            /// </summary>
            public uint e_lfanew;

            /// <summary>
            /// The e_lfarlc. File address of relocation table.
            /// </summary>
            public ushort e_lfarlc;

            /// <summary>
            /// The e_magic. Magic number.
            /// </summary>
            public ushort e_magic;

            /// <summary>
            /// The e_maxalloc. Maximum extra paragraphs needed.
            /// </summary>
            public ushort e_maxalloc;

            /// <summary>
            /// The e_minalloc. Minimum extra paragraphs needed.
            /// </summary>
            public ushort e_minalloc;

            /// <summary>
            /// The e_oemid. OEM identifier (for e_oeminfo).
            /// </summary>
            public ushort e_oemid;

            /// <summary>
            /// The e_oeminfo. OEM information; e_oemid specific.
            /// </summary>
            public ushort e_oeminfo;

            /// <summary>
            /// The e_ovno. Overlay number.
            /// </summary>
            public ushort e_ovno;

            /// <summary>
            /// The e_res 2_0. Reserved words.
            /// </summary>
            public ushort e_res2_0;

            /// <summary>
            /// The e_res 2_1. Reserved words.
            /// </summary>
            public ushort e_res2_1;

            /// <summary>
            /// The e_res 2_2. Reserved words.
            /// </summary>
            public ushort e_res2_2;

            /// <summary>
            /// The e_res 2_3. Reserved words.
            /// </summary>
            public ushort e_res2_3;

            /// <summary>
            /// The e_res 2_4. Reserved words.
            /// </summary>
            public ushort e_res2_4;

            /// <summary>
            /// The e_res 2_5. Reserved words.
            /// </summary>
            public ushort e_res2_5;

            /// <summary>
            /// The e_res 2_6. Reserved words.
            /// </summary>
            public ushort e_res2_6;

            /// <summary>
            /// The e_res 2_7. Reserved words.
            /// </summary>
            public ushort e_res2_7;

            /// <summary>
            /// The e_res 2_8. Reserved words.
            /// </summary>
            public ushort e_res2_8;

            /// <summary>
            /// The e_res 2_9. Reserved words.
            /// </summary>
            public ushort e_res2_9;

            /// <summary>
            /// The e_res_0. Reserved words.
            /// </summary>
            public ushort e_res_0;

            /// <summary>
            /// The e_res_1. Reserved words.
            /// </summary>
            public ushort e_res_1;

            /// <summary>
            /// The e_res_2. Reserved words.
            /// </summary>
            public ushort e_res_2;

            /// <summary>
            /// The e_res_3. Reserved words.
            /// </summary>
            public ushort e_res_3;

            /// <summary>
            /// The e_sp. Initial SP value.
            /// </summary>
            public ushort e_sp;

            /// <summary>
            /// The e_ss. Initial (relative) SS value.
            /// </summary>
            public ushort e_ss;

            #endregion
        }

        #endregion

        #region IMAGE_OPTIONAL_HEADER32 struct

        /// <summary>
        /// Optional PE header for x86
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            /// <summary>
            /// The magic.
            /// </summary>
            public ushort Magic;

            /// <summary>
            /// The major linker version.
            /// </summary>
            public byte MajorLinkerVersion;

            /// <summary>
            /// The minor linker version.
            /// </summary>
            public byte MinorLinkerVersion;

            /// <summary>
            /// The size of code.
            /// </summary>
            public uint SizeOfCode;

            /// <summary>
            /// The size of initialized data.
            /// </summary>
            public uint SizeOfInitializedData;

            /// <summary>
            /// The size of uninitialized data.
            /// </summary>
            public uint SizeOfUninitializedData;

            /// <summary>
            /// The address of entry point.
            /// </summary>
            public uint AddressOfEntryPoint;

            /// <summary>
            /// The base of code.
            /// </summary>
            public uint BaseOfCode;

            /// <summary>
            /// The base of data.
            /// </summary>
            public uint BaseOfData;

            /// <summary>
            /// The image base.
            /// </summary>
            public uint ImageBase;

            /// <summary>
            /// The section alignment.
            /// </summary>
            public uint SectionAlignment;

            /// <summary>
            /// The file alignment.
            /// </summary>
            public uint FileAlignment;

            /// <summary>
            /// The major operating system version.
            /// </summary>
            public ushort MajorOperatingSystemVersion;

            /// <summary>
            /// The minor operating system version.
            /// </summary>
            public ushort MinorOperatingSystemVersion;

            /// <summary>
            /// The major image version.
            /// </summary>
            public ushort MajorImageVersion;

            /// <summary>
            /// The minor image version.
            /// </summary>
            public ushort MinorImageVersion;

            /// <summary>
            /// The major subsystem version.
            /// </summary>
            public ushort MajorSubsystemVersion;

            /// <summary>
            /// The minor subsystem version.
            /// </summary>
            public ushort MinorSubsystemVersion;

            /// <summary>
            /// The win 32 version value.
            /// </summary>
            public uint Win32VersionValue;

            /// <summary>
            /// The size of image.
            /// </summary>
            public uint SizeOfImage;

            /// <summary>
            /// The size of headers.
            /// </summary>
            public uint SizeOfHeaders;

            /// <summary>
            /// The check sum.
            /// </summary>
            public uint CheckSum;

            /// <summary>
            /// The subsystem.
            /// </summary>
            public ushort Subsystem;

            /// <summary>
            /// The dll characteristics.
            /// </summary>
            public ushort DllCharacteristics;

            /// <summary>
            /// The size of stack reserve.
            /// </summary>
            public uint SizeOfStackReserve;

            /// <summary>
            /// The size of stack commit.
            /// </summary>
            public uint SizeOfStackCommit;

            /// <summary>
            /// The size of heap reserve.
            /// </summary>
            public uint SizeOfHeapReserve;

            /// <summary>
            /// The size of heap commit.
            /// </summary>
            public uint SizeOfHeapCommit;

            /// <summary>
            /// The loader flags.
            /// </summary>
            public uint LoaderFlags;

            /// <summary>
            /// The number of rva and sizes.
            /// </summary>
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
            /// <summary>
            /// The magic.
            /// </summary>
            public ushort Magic;

            /// <summary>
            /// The major linker version.
            /// </summary>
            public byte MajorLinkerVersion;

            /// <summary>
            /// The minor linker version.
            /// </summary>
            public byte MinorLinkerVersion;

            /// <summary>
            /// The size of code.
            /// </summary>
            public uint SizeOfCode;

            /// <summary>
            /// The size of initialized data.
            /// </summary>
            public uint SizeOfInitializedData;

            /// <summary>
            /// The size of uninitialized data.
            /// </summary>
            public uint SizeOfUninitializedData;

            /// <summary>
            /// The address of entry point.
            /// </summary>
            public uint AddressOfEntryPoint;

            /// <summary>
            /// The base of code.
            /// </summary>
            public uint BaseOfCode;

            /// <summary>
            /// The image base.
            /// </summary>
            public ulong ImageBase;

            /// <summary>
            /// The section alignment.
            /// </summary>
            public uint SectionAlignment;

            /// <summary>
            /// The file alignment.
            /// </summary>
            public uint FileAlignment;

            /// <summary>
            /// The major operating system version.
            /// </summary>
            public ushort MajorOperatingSystemVersion;

            /// <summary>
            /// The minor operating system version.
            /// </summary>
            public ushort MinorOperatingSystemVersion;

            /// <summary>
            /// The major image version.
            /// </summary>
            public ushort MajorImageVersion;

            /// <summary>
            /// The minor image version.
            /// </summary>
            public ushort MinorImageVersion;

            /// <summary>
            /// The major subsystem version.
            /// </summary>
            public ushort MajorSubsystemVersion;

            /// <summary>
            /// The minor subsystem version.
            /// </summary>
            public ushort MinorSubsystemVersion;

            /// <summary>
            /// The win 32 version value.
            /// </summary>
            public uint Win32VersionValue;

            /// <summary>
            /// The size of image.
            /// </summary>
            public uint SizeOfImage;

            /// <summary>
            /// The size of headers.
            /// </summary>
            public uint SizeOfHeaders;

            /// <summary>
            /// The check sum.
            /// </summary>
            public uint CheckSum;

            /// <summary>
            /// The subsystem.
            /// </summary>
            public ushort Subsystem;

            /// <summary>
            /// The dll characteristics.
            /// </summary>
            public ushort DllCharacteristics;

            /// <summary>
            /// The size of stack reserve.
            /// </summary>
            public ulong SizeOfStackReserve;

            /// <summary>
            /// The size of stack commit.
            /// </summary>
            public ulong SizeOfStackCommit;

            /// <summary>
            /// The size of heap reserve.
            /// </summary>
            public ulong SizeOfHeapReserve;

            /// <summary>
            /// The size of heap commit.
            /// </summary>
            public ulong SizeOfHeapCommit;

            /// <summary>
            /// The loader flags.
            /// </summary>
            public uint LoaderFlags;

            /// <summary>
            /// The number of rva and sizes.
            /// </summary>
            public uint NumberOfRvaAndSizes;
        }

        #endregion
    }
}
