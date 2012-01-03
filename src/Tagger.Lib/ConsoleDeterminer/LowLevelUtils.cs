﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedWinapi.Accessibility;
using System.Diagnostics;

namespace Tagger.Lib
{
    public static class LowLevelUtils
    {
        /// <summary>
        /// Check if a process is console application
        /// </summary>
        /// <param name="pid">Process ID of the checked process</param>
        /// <returns>true if process is a console application</returns>
        public static bool IsConsoleApp(int pid)
        {
            ushort IMAGE_SUBSYSTEM_WINDOWS_CUI = 3;
            var process = Process.GetProcessById(pid);
            var peParser = new PEParser(process.MainModule.FileName);

            if (peParser.Is32BitHeader)
            {
                return peParser.OptionalHeader32.Subsystem == IMAGE_SUBSYSTEM_WINDOWS_CUI;
            }
            else
            {
                return peParser.OptionalHeader64.Subsystem == IMAGE_SUBSYSTEM_WINDOWS_CUI;
            }
        }
    }
}
