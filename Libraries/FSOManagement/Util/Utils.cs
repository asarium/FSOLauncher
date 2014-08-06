#region Usings

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace FSOManagement.Util
{
    public static class Utils
    {
        public static string EnsureEndsWith(this string str, char c)
        {
            if (str.Length == 0)
                return new string(c, 1);

            if (str[str.Length - 1] != c)
            {
                str = str + c;
            }

            return str;
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> pred)
        {
            if (enumerable==null)throw new ArgumentNullException("enumerable");
            if (pred==null)throw new ArgumentNullException("pred");

            var index = 0;
            foreach (var val in enumerable)
            {
                if (pred(val))
                    return index;

                index++;
            }

            return -1;
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
        }
    }
}
