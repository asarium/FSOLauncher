#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Profiles;
using FSOManagement.Util;
using ReactiveUI;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class ModActivationManager : IModActivationManager
    {
        private IModification _activeMod;

        private string _commandLine;

        private IEnumerable<IModification> _primaryModifications = Enumerable.Empty<IModification>();

        private IEnumerable<IModification> _secondaryModifications = Enumerable.Empty<IModification>();

        public ModActivationManager([NotNull] Profile profile)
        {
            ParentProfile = profile;

            var modificationCountObservable =
                ParentProfile.WhenAnyObservable(x => x.SelectedTotalConversion.ModManager.Modifications.CountChanged);

            if (ParentProfile.SelectedModification != null)
            {
                // We need to wait until the mods have been loaded, then the
                // selected modification can be resolved
                modificationCountObservable.Where(count => count > 0).FirstAsync().Subscribe(_ =>
                {
                    if (ParentProfile.SelectedTotalConversion == null)
                    {
                        return;
                    }

                    var mods = ParentProfile.SelectedTotalConversion.ModManager.Modifications;

                    ActiveMod = GetModification(mods, ParentProfile.SelectedModification);
                });
            }

            this.WhenAnyValue(x => x.ActiveMod.Dependencies).CombineLatest(modificationCountObservable, (deps, mods) => deps)
                .Subscribe(deps =>
                {
                    PrimaryModifications = deps.PrimaryDependencies.Select(ResolveModification).ToList();

                    SecondaryModifications = deps.SecondayDependencies.Select(ResolveModification).ToList();
                });

            this.WhenAny(x => x.ActiveMod, x => x.PrimaryModifications, x => x.SecondaryModifications,
                (val1, val2, val3) => GetCommandLine(val1.Value, val2.Value, val3.Value)).BindTo(this, x => x.CommandLine);

            this.WhenAnyValue(x => x.ActiveMod.ModRootPath).BindTo(ParentProfile, x => x.SelectedModification);
        }

        [NotNull]
        public Profile ParentProfile { get; private set; }

        #region IModActivationManager Members

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

        public IEnumerable<IModification> PrimaryModifications
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

        public IEnumerable<IModification> SecondaryModifications
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

        public IModification ActiveMod
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
        private static IModification GetModification([NotNull] IEnumerable<IModification> modifications, [CanBeNull] string modFolder)
        {
            if (modFolder == null)
            {
                return null;
            }

            return modifications.FirstOrDefault(mod => Utils.NormalizePath(mod.ModRootPath) == Utils.NormalizePath(modFolder));
        }

        [NotNull]
        private string GetModArgument([NotNull] IModification mod)
        {
            if (ParentProfile.SelectedTotalConversion == null)
            {
                return Path.GetFileName(mod.ModRootPath);
            }

            var rootPath = ParentProfile.SelectedTotalConversion.RootFolder.EnsureEndsWith(Path.DirectorySeparatorChar);
            var modPath = mod.ModRootPath.EnsureEndsWith(Path.DirectorySeparatorChar);

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
        private IEnumerable<string> GetCommandLineFragments([CanBeNull] IModification mod, [NotNull] IEnumerable<IModification> primary,
            [NotNull] IEnumerable<IModification> secondary)
        {
            if (mod == null)
            {
                yield break;
            }

            if (ParentProfile.SelectedTotalConversion == null)
            {
                yield return GetModArgument(mod);

                yield break;
            }

            if (Utils.NormalizePath(mod.ModRootPath) == Utils.NormalizePath(ParentProfile.SelectedTotalConversion.RootFolder))
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
        private string GetCommandLine([CanBeNull] IModification mod, [NotNull] IEnumerable<IModification> primary,
            [NotNull] IEnumerable<IModification> secondary)
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
        private IModification ResolveModification([NotNull] string path)
        {
            if (ParentProfile.SelectedTotalConversion == null)
            {
                return null;
            }

            var modPath = Path.GetFullPath(Path.Combine(ParentProfile.SelectedTotalConversion.RootFolder, path));

            return
                ParentProfile.SelectedTotalConversion.ModManager.Modifications.FirstOrDefault(
                    mod => string.Equals(mod.ModRootPath, modPath, StringComparison.InvariantCultureIgnoreCase));
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
