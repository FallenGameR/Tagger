// -----------------------------------------------------------------------
// <copyright file="Window.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tagger.Dwm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class WindowItem
    {
        public string Title;
        public IntPtr Handle;

        public override string ToString()
        {
            return Title;
        }
    }
}
