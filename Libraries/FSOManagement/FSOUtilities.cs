#region Usings

using System.IO;

#endregion

namespace FSOManagement
{
    public enum GameDataType
    {
        Unknown,

        FS2,

        Diaspora,

        TBP
    }

    public static class FSOUtilities
    {
        public static GameDataType GetGameTypeFromPath(string path)
        {
            // This has to be checked first as TBP ships with a root_fs2.vp file
            if (File.Exists(Path.Combine(path, "B5-Core-3_4.vp")))
            {
                return GameDataType.TBP;
            }

            if (File.Exists(Path.Combine(path, "Root_fs2.vp")))
            {
                return GameDataType.FS2;
            }

            if (File.Exists(Path.Combine(path, "R1_Core.vp")))
            {
                return GameDataType.Diaspora;
            }

            return GameDataType.Unknown;
        }
    }
}
