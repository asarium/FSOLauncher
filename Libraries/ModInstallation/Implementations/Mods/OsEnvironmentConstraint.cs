#region Usings

using System;
using System.IO;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class OsEnvironmentConstraint : IEnvironmentConstraint
    {
        private readonly OsType _osType;

        public OsEnvironmentConstraint(OsType osType)
        {
            _osType = osType;
        }

        #region IEnvironmentConstraint Members

        public bool EnvironmentSatisfied()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users") && Directory.Exists("/Volumes"))
                    {
                        return _osType == OsType.Macos;
                    }
                    
                    return _osType == OsType.Linux;

                case PlatformID.MacOSX:
                    return _osType == OsType.Macos;

                default:
                    return _osType == OsType.Windows;
            }
        }

        #endregion
    }
}
