#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace FSOManagement.Util
{
    [StructLayout(LayoutKind.Sequential), Serializable]
    internal struct Flag
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string name; // The actual flag

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string desc; // The text that will appear in the launcher (unless its blank, other name is shown)

        [MarshalAs(UnmanagedType.U1)]
        public bool fso_only; // true if this is a fs2_open only feature

        public int off_flags; // Easy flag which will turn this feature off

        public int on_flags; // Easy flag which will turn this feature on

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string type; // Launcher uses this to put flags under different headings

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string web_url; // Link to documentation of feature (please use wiki or somewhere constant)
    };

    [StructLayout(LayoutKind.Sequential), Serializable]
    internal struct EasyFlag
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;
    }
}
