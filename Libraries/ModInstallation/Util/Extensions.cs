using System;
using ModInstallation.Annotations;

namespace ModInstallation.Util
{
    public static class Extensions
    {
        [NotNull]
        public static string HumanReadableByteCount(this long bytes, bool si)
        {
            var unit = si ? 1000 : 1024;
            if (bytes < unit)
            {
                return bytes + " B";
            }

            var exp = (int) (Math.Log(bytes) / Math.Log(unit));
            var pre = (si ? "kMGTPE" : "KMGTPE")[exp - 1] + (si ? "" : "i");

            return string.Format("{0:0.0} {1}B", bytes / Math.Pow(unit, exp), pre);
        }
    }
}