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
using ReactiveUI;

#endregion

namespace FSOManagement.Profiles
{
    public class ModActivationManager : IModActivationManager
    {
        private readonly Profile _parentProfile;

        private Modification _activeMod;

        private string _commandLine;

        private IEnumerable<Modification> _primaryModifications = Enumerable.Empty<Modification>();

        private IEnumerable<Modification> _secondaryModifications = Enumerable.Empty<Modification>();

        public ModActivationManager([NotNull] Profile profile)
        {
            _parentProfile = profile;

            _parentProfile.WhenAny(x => x.SelectedTotalConversion, x => x.SelectedModification,
                (val1, val2) => val1.Value == null ? null : GetModification(val1.Value.ModManager.Modifications, val2.Value))
                .BindTo(this, x => x.ActiveMod);

            this.WhenAny(x => x.ActiveMod, val => val.Value == null ? null : val.Value.ModFolderPath)
                .BindTo(_parentProfile, x => x.SelectedModification);

            this.WhenAnyValue(x => x.ActiveMod.Dependencies).Subscribe(deps =>
            {
                PrimaryModifications = deps.PrimaryDependencies.Select(ResolveModification).ToList();

                SecondaryModifications = deps.SecondayDependencies.Select(ResolveModification).ToList();
            });

            this.WhenAny(x => x.ActiveMod, x => x.PrimaryModifications, x => x.SecondaryModifications,
                (val1, val2, val3) => GetCommandLine(val1.Value, val2.Value, val3.Value)).BindTo(this, x => x.CommandLine);
        }

        #region IModActivationManager Members

        [NotNull]
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

        [NotNull]
        public IEnumerable<Modification> PrimaryModifications
        {
            get { return _primaryModifications; }
            private set
            {
                if (Equals(value, _primaryModifications))
                {
                    return;
                }
                _primaryModifications = value;
                OnPropertyChanged();
            }
        }

        [NotNull]
        public IEnumerable<Modification> SecondaryModifications
        {
            get { return _secondaryModifications; }
            private set
            {
                if (Equals(value, _secondaryModifications))
                {
                    return;
                }
                _secondaryModifications = value;
                OnPropertyChanged();
            }
        }

        [CanBeNull]
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
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [CanBeNull]
        private static Modification GetModification([NotNull] IEnumerable<Modification> modifications, [CanBeNull] string modFolder)
        {
            if (modFolder == null)
            {
                return null;
            }

            return modifications.FirstOrDefault(mod => Utils.NormalizePath(mod.ModFolderPath) == Utils.NormalizePath(modFolder));
        }

        [NotNull]
        private string GetModArgument([NotNull] Modification mod)
        {
            if (_parentProfile.SelectedTotalConversion == null)
            {
                return mod.FolderName;
            }

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

        [NotNull]
        private IEnumerable<string> GetCommandLineFragments([CanBeNull] Modification mod, [NotNull] IEnumerable<Modification> primary,
            [NotNull] IEnumerable<Modification> secondary)
        {
            if (mod == null)
            {
                yield break;
            }

            if (_parentProfile.SelectedTotalConversion == null)
            {
                yield return GetModArgument(mod);

                yield break;
            }

            if (Utils.NormalizePath(mod.ModFolderPath) == Utils.NormalizePath(_parentProfile.SelectedTotalConversion.RootFolder))
            {
                // No arguments when the mod is the root.
                yield break;
            }

            foreach (var arg in primary.Select(GetModArgument))
            {
                yield return arg;
            }

            yield return GetModArgument(mod);

            foreach (var arg in secondary.Select(GetModArgument))
            {
                yield return arg;
            }
        }

        [NotNull]
        private string GetCommandLine([CanBeNull] Modification mod, [NotNull] IEnumerable<Modification> primary,
            [NotNull] IEnumerable<Modification> secondary)
        {
            var fragments = GetCommandLineFragments(mod, primary, secondary);

            var fragmentString = string.Join(",", fragments);

            if (fragmentString.Contains(" "))
            {
                fragmentString = string.Format("\"{0}\"", fragmentString);
            }

            return fragmentString.Length > 0 ? string.Format("-mod {0}", fragmentString) : "";
        }

        [CanBeNull]
        private Modification ResolveModification([NotNull] string path)
        {
            if (_parentProfile.SelectedTotalConversion == null)
            {
                return null;
            }

            var modPath = Path.GetFullPath(Path.Combine(_parentProfile.SelectedTotalConversion.RootFolder, path));

            return
                _parentProfile.SelectedTotalConversion.ModManager.Modifications.FirstOrDefault(
                    mod => string.Equals(mod.ModFolderPath, modPath, StringComparison.InvariantCultureIgnoreCase));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CanBeNull, CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
