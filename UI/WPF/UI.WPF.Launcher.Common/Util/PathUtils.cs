using System;
using System.IO;

namespace UI.WPF.Launcher.Common.Util
{
    public static class PathUtils
    {
        public static bool PathEquals(this string path1, string path2)
        {
            try
            {
                var fullPath1 = Path.GetFullPath(path1);
                var fullPath2 = Path.GetFullPath(path2);

                return fullPath1 == fullPath2;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}