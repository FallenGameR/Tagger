using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using Tagger.WinAPI;
using System.Diagnostics;

namespace Tagger
{
    /// <summary>
    /// Reads in the header information of the Portable Executable format.
    /// Provides information such as the date the assembly was compiled. 
    /// Original from http://code.cheesydesign.com/?p=572
    /// </summary>
    public class ConsoleDeterminer
    {
        #region Fields

        /// <summary>
        /// The DOS header
        /// </summary>
        private NativeAPI.IMAGE_DOS_HEADER dosHeader;

        /// <summary>
        /// The file header
        /// </summary>
        private NativeAPI.IMAGE_FILE_HEADER fileHeader;

        /// <summary>
        /// Optional 32 bit file header
        /// </summary>
        private NativeAPI.IMAGE_OPTIONAL_HEADER32 optionalHeader32;

        /// <summary>
        /// Optional 64 bit file header
        /// </summary>
        private NativeAPI.IMAGE_OPTIONAL_HEADER64 optionalHeader64;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleDeterminer(string filePath)
        {
            // Read in the DLL or EXE and get the timestamp
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                // Read DOS header
                this.dosHeader = ConsoleDeterminer.ReadStructure<NativeAPI.IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(this.dosHeader.e_lfanew, SeekOrigin.Begin);

                // Skip NT header signature 
                UInt32 ntHeadersSignature = reader.ReadUInt32();

                // Read file header
                this.fileHeader = ConsoleDeterminer.ReadStructure<NativeAPI.IMAGE_FILE_HEADER>(reader);

                // Read optional header
                if (this.Is32BitOptionalHeader)
                {
                    this.optionalHeader32 = ConsoleDeterminer.ReadStructure<NativeAPI.IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    this.optionalHeader64 = ConsoleDeterminer.ReadStructure<NativeAPI.IMAGE_OPTIONAL_HEADER64>(reader);
                }
            }
        }

        public static uint GetPid(IntPtr handle)
        {
            // TODO: Move to domain logic
            uint pid;
            NativeAPI.GetWindowThreadProcessId(handle, out pid);

            // Get actual process ID belonging to host window
            bool isConsoleApp = ConsoleDeterminer.IsConsoleApplication((int)pid);
            if (isConsoleApp)
            {
                using (var wct = new ProcessFinder())
                {
                    pid = (uint)wct.GetConhostProcess((int)pid);
                }
            }

            return pid;
        }

        /// <summary>
        /// Checks if the application specified by process ID is a console application
        /// </summary>
        /// <param name="pid">Process ID for checked application</param>
        /// <returns>true is application is build as console application</returns>
        public static bool IsConsoleApplication(int pid)
        {
            var process = Process.GetProcessById(pid);
            var parser = new ConsoleDeterminer(process.MainModule.FileName);
            return parser.Subsystem == (ushort)NativeAPI.IMAGE_SUBSYSTEM_WINDOWS.CUI;
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
        /// Gets if the file header is 32 bit or not
        /// </summary>
        public bool Is32BitOptionalHeader
        {
            get { return (NativeAPI.IMAGE_FILE_32BIT_MACHINE & this.fileHeader.Characteristics) != 0; }
        }

        /// <summary>
        /// Image subsystem the binary was build against
        /// </summary>
        public ushort Subsystem
        {
            get
            {
                return this.Is32BitOptionalHeader
                    ? this.optionalHeader32.Subsystem
                    : this.optionalHeader64.Subsystem;
            }
        }

        #endregion
    }
}
