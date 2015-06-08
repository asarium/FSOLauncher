#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ModInstallation.Interfaces.Mods;
using Semver;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class RetailModification : IInstalledModification
    {
        public RetailModification(string installPath)
        {
            InstallPath = installPath;
        }

        #region IInstalledModification Members

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IEquatable<IModification>

        public bool Equals(IModification other)
        {
            return string.Equals(Id, other.Id);
        }

        #endregion

        #region Implementation of IInstalledModification

        public string InstallPath { get; private set; }

        #endregion

        #endregion

        #region Implementation of IModification

        public string Title
        {
            get { return "FreeSpace Retail"; }
        }

        public SemVersion Version
        {
            get { return new SemVersion(1); }
        }

        public string Id
        {
            get { return "retail"; }
        }

        public string Commandline
        {
            get { return ""; }
        }

        public string FolderName
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IPackage> Packages
        {
            get { return Enumerable.Empty<IPackage>(); }
        }

        public string Description
        {
            get { return "Retail FreeSpace 2, no mods."; }
        }

        public Uri LogoUri {
            get { return new Uri(Path.Combine(InstallPath, "FS2.bmp")); }
        }

        public IPostInstallActions PostInstallActions
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
