#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Util;

#endregion

namespace FSOManagement.Profiles
{
    public class ModActivationManager : IModActivationManager
    {
        [field: NonSerialized]
        private readonly Profile _parentProfile;

        private Modification _activeMod;

        private string _commandLine;

        private IList<Modification> _primaryModifications;

        private IList<Modification> _secondaryModifications;

        public ModActivationManager(Profile profile)
        {
            _parentProfile = profile;
        }

        public string CommandLine
        {
            get { return _commandLine; }
            private set
            {
                if (value == _commandLine)
                {
                    return;
                }
                _commandLine = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Modification> PrimaryModifications
        {
            get { return _primaryModifications; }
        }

        public IEnumerable<Modification> SecondaryModifications
        {
            get { return _secondaryModifications; }
        }

        public Modification ActiveMod
        {
            get { return _activeMod; }
            set
            {
                if (Equals(value, _activeMod))
                {
                    return;
                }
                _activeMod = value;
                OnPropertyChanged();

                _parentProfile.SelectedModification = value;

                RefreshModLists();
            }
        }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void RefreshModLists()
        {
            _primaryModifications = ActiveMod.Dependencies.PrimaryDependencies.Select(ResolveModification).ToList();

            _secondaryModifications = ActiveMod.Dependencies.SecondayDependencies.Select(ResolveModification).ToList();

            UpdateCommandLine();
        }

        private string GetModArgument(Modification mod)
        {
            var rootPath = _parentProfile.SelectedTotalConversion.RootFolder.EnsureEndsWith(Path.DirectorySeparatorChar);
            var modPath = mod.ModFolderPath.EnsureEndsWith(Path.DirectorySeparatorChar);

            var rootUri = new Uri(rootPath, UriKind.Absolute);
            var modUri = new Uri(modPath, UriKind.Absolute);

            var relativeUri = rootUri.MakeRelativeUri(modUri);

            var relativePath = relativeUri.ToString();

            // MakeRelativeUri appends a / at the end
            if (relativePath.Length > 0 && relativePath[relativePath.Length - 1] == '/')
            {
                relativePath = relativePath.Substring(0, relativePath.Length - 1);
            }

            return Uri.UnescapeDataString(relativePath);
        }

        private IEnumerable<string> GetCommandLineFragments()
        {
            if (Utils.NormalizePath(ActiveMod.ModFolderPath) == Utils.NormalizePath(_parentProfile.SelectedTotalConversion.RootFolder))
            {
                // No arguments when the mod is the root.
                yield break;
            }

            foreach (var arg in PrimaryModifications.Select(GetModArgument))
            {
                yield return arg;
            }

            yield return GetModArgument(ActiveMod);

            foreach (var arg in SecondaryModifications.Select(GetModArgument))
            {
                yield return arg;
            }
        }

        private void UpdateCommandLine()
        {
            var fragments = GetCommandLineFragments();

            var fragmentString = string.Join(",", fragments);

            if (fragmentString.Contains(" "))
            {
                fragmentString = string.Format("\"{0}\"", fragmentString);
            }

            CommandLine = fragmentString.Length > 0 ? string.Format("-mod {0}", fragmentString) : "";
        }

        private Modification ResolveModification(string path)
        {
            var modPath = Path.GetFullPath(Path.Combine(_parentProfile.SelectedTotalConversion.RootFolder, path));

            return
                _parentProfile.SelectedTotalConversion.ModManager.Modifications.FirstOrDefault(
                    mod => string.Equals(mod.ModFolderPath, modPath, StringComparison.InvariantCultureIgnoreCase));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
