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
using MoreLinq;
using ReactiveUI;
using Splat;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class ModActivationManager : IModActivationManager, IEnableLogger
    {
        private IEnumerable<ILocalModification> _activatedMods = Enumerable.Empty<ILocalModification>();

        private ILocalModification _activeMod;

        private string _commandLine;

        public ModActivationManager([NotNull] Profile profile)
        {
            ParentProfile = profile;

            var modificationCountObservable = ParentProfile.WhenAnyObservable(x => x.SelectedTotalConversion.ModManager.ModificationLists.CountChanged);
            var selectedModObservable = ParentProfile.WhenAnyValue(x => x.SelectedModification);

            ModDependencyManagers = Locator.Current.GetServices<IModDependencies>();

            modificationCountObservable.CombineLatest(selectedModObservable,
                (count, mod) => new
                {
                    Count = count,
                    Mod = mod
                }).Where(x => x.Count > 0 && x.Mod != null).Subscribe(x =>
                {
                    if (ParentProfile.SelectedTotalConversion == null)
                    {
                        return;
                    }

                    ActiveMod = GetModification(ParentProfile.SelectedTotalConversion.ModManager.GetModifications(), x.Mod);
                });

            this.WhenAnyValue(x => x.ActiveMod)
                .CombineLatest(profile.WhenAnyValue(x => x.SelectedTotalConversion.RootFolder),
                    modificationCountObservable,
                    (mod, root, mods) => new
                    {
                        mod,
                        root
                    })
                .Where(x => x.mod != null)
                .Subscribe(val => ActivatedMods = GetModPaths(val.mod, val.root).Select(ResolveModification).ToList());

            this.WhenAnyValue(x => x.ActiveMod, x => x.ActivatedMods)
                .Select(val => GetCommandLine(val.Item1, val.Item2))
                .BindTo(this, x => x.CommandLine);

            this.WhenAnyValue(x => x.ActiveMod.ModRootPath).BindTo(ParentProfile, x => x.SelectedModification);
        }

        private IEnumerable<IModDependencies> ModDependencyManagers { get; set; }

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

        public IEnumerable<ILocalModification> ActivatedMods
        {
            get { return _activatedMods; }
            private set
            {
                if (Equals(value, _activatedMods))
                {
                    return;
                }
                _activatedMods = value;
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

        private IEnumerable<string> GetModPaths(ILocalModification mod, string rootPath)
        {
            var bestMatch = ModDependencyManagers.MaxBy(deps => deps.GetSupportScore(mod));
            try
            {
                return bestMatch.GetModPaths(mod, rootPath);
            }
            catch (NotSupportedException e)
            {
                this.Log().ErrorException("No dependency manager of mod type '" + mod.GetType().FullName + "' found!", e);
                return mod.ModRootPath.AsEnumerable();
            }
        }

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

            if (Utils.NormalizePath(mod.ModRootPath) == Utils.NormalizePath(ParentProfile.SelectedTotalConversion.RootFolder))
            {
                // No arguments when the mod is the root.
                return null;
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
        private IEnumerable<string> GetCommandLineFragments([NotNull] IEnumerable<ILocalModification> allModification)
        {
            return allModification.Select(GetModArgument).Where(item => item != null);
        }

        [NotNull]
        private string GetCommandLine([CanBeNull] ILocalModification mod, [NotNull] IEnumerable<ILocalModification> allModifications)
        {
            var fragments = GetCommandLineFragments(allModifications);

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
