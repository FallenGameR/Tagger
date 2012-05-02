//-----------------------------------------------------------------------
// <copyright file="PortableExecutableReader.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Tagger.WinAPI;

    /// <summary>
    /// Reads Portable Executable header info
    /// </summary>
    /// <remarks>
    /// See also http://code.cheesydesign.com/?p=572 "Reading the Portable Executable (PE) header in C#"
    /// </remarks>
    public class PortableExecutableReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortableExecutableReader"/> class. 
        /// </summary>
        /// <param name="filePath">Path to the file to read</param>
        public PortableExecutableReader(string filePath)
        {
            // Read in the DLL or EXE and get the timestamp
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                // Read DOS header
                this.DosHeader = ReadStructure<NativeMethods.IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(this.DosHeader.e_lfanew, SeekOrigin.Begin);

                // Skip NT header signature 
                reader.ReadUInt32();

                // Read file header
                this.FileHeader = ReadStructure<NativeMethods.IMAGE_FILE_HEADER>(reader);

                // Read optional header
                if (this.IsOptionalHeader32)
                {
                    this.OptionalHeader32 = ReadStructure<NativeMethods.IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    this.OptionalHeader64 = ReadStructure<NativeMethods.IMAGE_OPTIONAL_HEADER64>(reader);
                }
            }
        }

        /// <summary>
        /// Gets DOS header
        /// </summary>
        public NativeMethods.IMAGE_DOS_HEADER DosHeader { get; private set; }

        /// <summary>
        /// Gets file header
        /// </summary>
        public NativeMethods.IMAGE_FILE_HEADER FileHeader { get; private set; }

        /// <summary>
        /// Gets optional 32 bit file header
        /// </summary>
        public NativeMethods.IMAGE_OPTIONAL_HEADER32 OptionalHeader32 { get; private set; }

        /// <summary>
        /// Gets optional 64 bit file header
        /// </summary>
        public NativeMethods.IMAGE_OPTIONAL_HEADER64 OptionalHeader64 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the file header is 32 bit or not
        /// </summary>
        public bool IsOptionalHeader32
        {
            get { return (NativeMethods.IMAGE_FILE_32BIT_MACHINE & this.FileHeader.Characteristics) != 0; }
        }

        /// <summary>
        /// Gets optional header without bitwise distinction
        /// </summary>
        public dynamic OptionalHeader
        {
            get
            {
                return this.IsOptionalHeader32
                    ? (dynamic)this.OptionalHeader32
                    : (dynamic)this.OptionalHeader64;
            }
        }

        /// <summary>
        /// Reads in a block from a file and converts it to the struct 
        /// type specified by the template parameter
        /// </summary>
        /// <typeparam name="T">Type of the output structure</typeparam>
        /// <param name="reader">Reader object that is used to read file</param>
        /// <returns>Structure read from the current file position</returns>
        private static T ReadStructure<T>(BinaryReader reader)
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }
    }
}
