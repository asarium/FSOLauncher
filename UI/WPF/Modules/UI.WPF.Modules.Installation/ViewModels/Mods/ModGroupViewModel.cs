﻿#region Usings

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using Semver;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class ModGroupViewModel : ReactiveObjectBase
    {
        private readonly IModGroup<IModification> _group;

        private ModViewModel _currentMod;

        private bool? _isSelected;

        private SemVersion _selectedVersion;

        public ModGroupViewModel(IModGroup<IModification> group, IModInstallationManager modInstallationManager)
        {
            _group = @group;

            this.WhenAnyValue(x => x.SelectedVersion)
                .Where(x => x != null && _group.Versions.ContainsKey(x))
                .Select(x => new ModViewModel(_group.Versions[x], modInstallationManager))
                .BindTo(this, x => x.CurrentMod);

            // When selected changes, propagate to the mod view model
            this.WhenAnyValue(x => x.CurrentMod.ModSelected).BindTo(this, x => x.IsSelected);
            this.WhenAnyValue(x => x.IsSelected).BindTo(this, x => x.CurrentMod.ModSelected);

            SelectedVersion = _group.Versions.Keys.Max();

            Versions = group.Versions.Keys.OrderByDescending(x => x).ToList();
            HasMultipleVersions = Versions.Count() > 1;
        }

        public IEnumerable<SemVersion> Versions { get; private set; }

        public bool HasMultipleVersions { get; private set; }

        public bool? IsSelected
        {
            get { return _isSelected; }
            set { RaiseAndSetIfPropertyChanged(ref _isSelected, value); }
        }

        public SemVersion SelectedVersion
        {
            get { return _selectedVersion; }
            set { RaiseAndSetIfPropertyChanged(ref _selectedVersion, value); }
        }

        public ModViewModel CurrentMod
        {
            get { return _currentMod; }
            private set { RaiseAndSetIfPropertyChanged(ref _currentMod, value); }
        }

        public IModGroup<IModification> Group
        {
            get { return _group; }
        }
    }
}
