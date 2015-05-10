#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Installation.Interfaces;
using UI.WPF.Modules.Installation.ViewModels.ListOverview;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 3), Export(typeof(IInstallationManager))]
    public class InstallationTabViewModel : Screen, ILauncherTab, IInstallationManager, IHandle<MainWindowOpenedMessage>
    {
        private IInstallationState _currentState;

        [ImportingConstructor]
        public InstallationTabViewModel(ILocalModManager localModManager, IRemoteModManager remoteModManager, IEventAggregator aggregator)
        {
            LocalModManager = localModManager;
            RemoteModManager = remoteModManager;

            _modGroups = new ReactiveList<IModGroup>();

            UpdatePackageListCommand = ReactiveCommand.CreateAsyncTask(_ => UpdatePackageList());

            this.WhenAnyValue(x => x.ManagerStatusMessage).Select(val => !string.IsNullOrEmpty(val)).BindTo(this, x => x.HasManagerStatusMessage);

            CurrentState = InitializeFirstState();

            aggregator.Subscribe(this);
        }

        public ILocalModManager LocalModManager { get; set; }

        public IRemoteModManager RemoteModManager { get; set; }

        #region ILauncherTab Members

        #region Overrides of Screen

        public override string DisplayName
        {
            get { return "Update/Install Mods"; }
        }

        #endregion

        #endregion

        private IInstallationState InitializeFirstState()
        {
            return new PackageListViewModel(this);
        }

        #region Implementation of IInstallationManager

        public bool HasManagerStatusMessage
        {
            get { return _hasManagerStatusMessage; }
            private set
            {
                if (value.Equals(_hasManagerStatusMessage))
                {
                    return;
                }
                _hasManagerStatusMessage = value;
                NotifyOfPropertyChange();
            }
        }

        [CanBeNull]
        public string ManagerStatusMessage
        {
            get { return _managerStatusMessage; }
            private set
            {
                if (value == _managerStatusMessage)
                {
                    return;
                }
                _managerStatusMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public IInstallationState CurrentState
        {
            get { return _currentState; }
            private set
            {
                if (Equals(value, _currentState))
                {
                    return;
                }
                _currentState = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand UpdatePackageListCommand { get; private set; }

        public IReadOnlyReactiveList<IModGroup> ModGroups
        {
            get { return _modGroups; }
        }

        public async Task UpdatePackageList()
        {
            await Task.Delay(10000);

            ManagerStatusMessage = "Parsing local mod information...";
            await LocalModManager.ParseLocalModDataAsync().ConfigureAwait(false);

            var modGroups =
                await
                    RemoteModManager.GetModGroupsAsync(new Progress<string>(msg => ManagerStatusMessage = msg), true, CancellationToken.None)
                        .ConfigureAwait(false);

            if (modGroups != null)
            {
                _modGroups.Reset();
                _modGroups.AddRange(modGroups);
            }
            else
            {
                var result =
                    await
                        UserError.Throw(new UserError("Mod information retrieval failed!",
                            "There was an error while retrieving the modification information.\n" +
                            "Please check if your internet connection is working.",
                            new[]
                            {
                                new RecoveryCommand("Retry", _ => RecoveryOptionResult.RetryOperation)
                                {
                                    IsDefault = true
                                },
                                new RecoveryCommand("Cancel", _ => RecoveryOptionResult.CancelOperation)
                            }));

                switch (result)
                {
                    case RecoveryOptionResult.CancelOperation:
                        break;
                    case RecoveryOptionResult.RetryOperation:
                        UpdatePackageListCommand.Execute(null);
                        break;
                }
            }

            ManagerStatusMessage = null;
        }

        public Task InstallPackages(IEnumerable<IPackage> packages)
        {
            throw new NotImplementedException();
        }

        private readonly ReactiveList<IModGroup> _modGroups;

        private bool _hasManagerStatusMessage;

        private string _managerStatusMessage;


        #endregion

        #region Implementation of IHandle<MainWindowOpenedMessage>

        void IHandle<MainWindowOpenedMessage>.Handle(MainWindowOpenedMessage message)
        {
            UpdatePackageListCommand.Execute(null);
        }

        #endregion
    }
}
