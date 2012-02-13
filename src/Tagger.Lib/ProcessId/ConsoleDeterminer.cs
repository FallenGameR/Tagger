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
            using (var stream = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                BinaryReader reader = new BinaryReader(stream);
                dosHeader = FromBinaryReader<NativeAPI.IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                UInt32 ntHeadersSignature = reader.ReadUInt32();
                fileHeader = FromBinaryReader<NativeAPI.IMAGE_FILE_HEADER>(reader);
                if (this.Is32BitHeader)
                {
                    optionalHeader32 = FromBinaryReader<NativeAPI.IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    optionalHeader64 = FromBinaryReader<NativeAPI.IMAGE_OPTIONAL_HEADER64>(reader);
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
            return parser.Subsystem == (ushort) NativeAPI.IMAGE_SUBSYSTEM_WINDOWS.CUI;
        }

        /// <summary>
        /// Reads in a block from a file and converts it to the struct 
        /// type specified by the template parameter
        /// </summary>
        private static T FromBinaryReader<T>(BinaryReader reader)
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
        public bool Is32BitHeader
        {
            get
            {
                UInt16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
                return (IMAGE_FILE_32BIT_MACHINE & FileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
            }
        }

        /// <summary>
        /// Gets the file header
        /// </summary>
        public NativeAPI.IMAGE_FILE_HEADER FileHeader
        {
            get { return fileHeader; }
        }

        /// <summary>
        /// Gets the optional header
        /// </summary>
        public NativeAPI.IMAGE_OPTIONAL_HEADER32 OptionalHeader32
        {
            get { return optionalHeader32; }
        }

        /// <summary>
        /// Gets the optional header
        /// </summary>
        public NativeAPI.IMAGE_OPTIONAL_HEADER64 OptionalHeader64
        {
            get { return optionalHeader64; }
        }

        /// <summary>
        /// Gets the timestamp from the file header
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                // Timestamp is a date offset from 1970
                DateTime returnValue = new DateTime(1970, 1, 1, 0, 0, 0);

                // Add in the number of seconds since 1970/1/1
                returnValue = returnValue.AddSeconds(fileHeader.TimeDateStamp);

                // Adjust to local timezone
                returnValue += TimeZone.CurrentTimeZone.GetUtcOffset(returnValue);

                return returnValue;
            }
        }

        /// <summary>
        /// Image subsystem the binary was build against
        /// </summary>
        public ushort Subsystem
        {
            get
            {
                return this.Is32BitHeader
                    ? this.OptionalHeader32.Subsystem
                    : this.OptionalHeader64.Subsystem;
            }
        }

        #endregion
    }
}
