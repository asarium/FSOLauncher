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
        private ILocalModification _activeMod;

        private string _commandLine;

        private IEnumerable<ILocalModification> _primaryModifications = Enumerable.Empty<ILocalModification>();

        private IEnumerable<ILocalModification> _secondaryModifications = Enumerable.Empty<ILocalModification>();

        public ModActivationManager([NotNull] Profile profile)
        {
            ParentProfile = profile;

            var modificationCountObservable = ParentProfile.WhenAnyObservable(x => x.SelectedTotalConversion.ModManager.ModificationLists.CountChanged);

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

                    ActiveMod = GetModification(ParentProfile.SelectedTotalConversion.ModManager.GetModifications(),
                        ParentProfile.SelectedModification);
                });
            }

            this.WhenAnyValue(x => x.ActiveMod.Dependencies)
                .CombineLatest(profile.WhenAnyValue(x => x.SelectedTotalConversion.RootFolder),
                    modificationCountObservable,
                    (deps, root, mods) => new
                    {
                        deps,
                        root
                    }).Subscribe(val =>
                    {
                        PrimaryModifications = val.deps.GetPrimaryDependencies(val.root).Select(ResolveModification).ToList();

                        SecondaryModifications = val.deps.GetSecondayDependencies(val.root).Select(ResolveModification).ToList();
                    });

            this.WhenAny(x => x.ActiveMod,
                x => x.PrimaryModifications,
                x => x.SecondaryModifications,
                (val1, val2, val3) => GetCommandLine(val1.Value, val2.Value, val3.Value)).BindTo(this, x => x.CommandLine);

            this.WhenAnyValue(x => x.ActiveMod.ModRootPath).BindTo(ParentProfile, x => x.SelectedModification);
        }

        [NotNull]
        private Profile ParentProfile { get; set; }

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

        public IEnumerable<ILocalModification> PrimaryModifications
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

        public IEnumerable<ILocalModification> SecondaryModifications
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

        public ILocalModification ActiveMod
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
        private static ILocalModification GetModification([NotNull] IEnumerable<ILocalModification> modifications, [CanBeNull] string modFolder)
        {
            if (modFolder == null)
            {
                return null;
            }

            var normalizedModFolder = Utils.NormalizePath(modFolder);

            return modifications.FirstOrDefault(checkMod => Utils.NormalizePath(checkMod.ModRootPath) == normalizedModFolder);
        }

        [NotNull]
        private string GetModArgument([NotNull] ILocalModification mod)
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
        private IEnumerable<string> GetCommandLineFragments([CanBeNull] ILocalModification mod,
            [NotNull] IEnumerable<ILocalModification> primary,
            [NotNull] IEnumerable<ILocalModification> secondary)
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
        private string GetCommandLine([CanBeNull] ILocalModification mod,
            [NotNull] IEnumerable<ILocalModification> primary,
            [NotNull] IEnumerable<ILocalModification> secondary)
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
        private ILocalModification ResolveModification([NotNull] string path)
        {
            if (ParentProfile.SelectedTotalConversion == null)
            {
                return null;
            }

            var modPath = Path.GetFullPath(path);

            return
                ParentProfile.SelectedTotalConversion.ModManager.GetModifications()
                    .FirstOrDefault(mod => string.Equals(mod.ModRootPath, modPath, StringComparison.InvariantCultureIgnoreCase));
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
