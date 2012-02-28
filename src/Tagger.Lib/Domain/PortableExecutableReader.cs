using System;
using System.IO;
using System.Runtime.InteropServices;
using Tagger.WinAPI;

namespace Tagger
{
    /// <summary>
    /// Reads Portable Executable header info
    /// </summary>
    /// <remarks>
    /// See also http://code.cheesydesign.com/?p=572 "Reading the Portable Executable (PE) header in C#"
    /// </remarks>
    public class PortableExecutableReader
    {
        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public PortableExecutableReader(string filePath)
        {
            // Read in the DLL or EXE and get the timestamp
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                // Read DOS header
                this.DosHeader = PortableExecutableReader.ReadStructure<NativeAPI.IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(this.DosHeader.e_lfanew, SeekOrigin.Begin);

                // Skip NT header signature 
                UInt32 ntHeadersSignature = reader.ReadUInt32();

                // Read file header
                this.FileHeader = PortableExecutableReader.ReadStructure<NativeAPI.IMAGE_FILE_HEADER>(reader);

                // Read optional header
                if (this.IsOptionalHeader32)
                {
                    this.OptionalHeader32 = PortableExecutableReader.ReadStructure<NativeAPI.IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    this.OptionalHeader64 = PortableExecutableReader.ReadStructure<NativeAPI.IMAGE_OPTIONAL_HEADER64>(reader);
                }
            }
        }

        /// <summary>
        /// Reads in a block from a file and converts it to the struct 
        /// type specified by the template parameter
        /// </summary>
        private static T ReadStructure<T>(BinaryReader reader)
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The DOS header
        /// </summary>
        public NativeAPI.IMAGE_DOS_HEADER DosHeader { get; private set; }

        /// <summary>
        /// The file header
        /// </summary>
        public NativeAPI.IMAGE_FILE_HEADER FileHeader { get; private set; }

        /// <summary>
        /// Optional 32 bit file header
        /// </summary>
        public NativeAPI.IMAGE_OPTIONAL_HEADER32 OptionalHeader32 { get; private set; }

        /// <summary>
        /// Optional 64 bit file header
        /// </summary>
        public NativeAPI.IMAGE_OPTIONAL_HEADER64 OptionalHeader64 { get; private set; }

        /// <summary>
        /// Gets if the file header is 32 bit or not
        /// </summary>
        public bool IsOptionalHeader32
        {
            get { return (NativeAPI.IMAGE_FILE_32BIT_MACHINE & this.FileHeader.Characteristics) != 0; }
        }

        /// <summary>
        /// Optional header without bitwise distinction
        /// </summary>
        public dynamic OptionalHeader
        {
            get
            {
                return this.IsOptionalHeader32
                    ? (dynamic) this.OptionalHeader32
                    : (dynamic) this.OptionalHeader64;
            }
        }

        #endregion
    }
}
