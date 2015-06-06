#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Exceptions;
using ModInstallation.Implementations.Util;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using Splat;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.Installation.Interfaces;
using UI.WPF.Modules.Installation.ViewModels.Installation;
using UI.WPF.Modules.Installation.ViewModels.ListOverview;
using UI.WPF.Modules.Installation.ViewModels.Operations;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    internal class ModDependencyException : DependencyException
    {
        public ModDependencyException(IModification mod) : base(mod.Title)
        {
            Mod = mod;
        }

        public IModification Mod { get; private set; }
    }

    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 3), Export(typeof(IInstallationManager))]
    public class InstallationTabViewModel : Screen, ILauncherTab, IInstallationManager, IEnableLogger
    {
        private readonly Subject<Unit> _activatedObservable = new Subject<Unit>();

        private readonly IReactiveList<IModGroup<IModification>> _modGroups = new ReactiveList<IModGroup<IModification>>();

        private readonly ICommand _updatePackageListCommand;

        private IInstallationState _currentState;

        private bool _hasManagerStatusMessage;

        private string _managerStatusMessage;

        [ImportingConstructor]
        public InstallationTabViewModel(IModInstallationManager modManager, IMessageBus bus)
        {
            ModInstallationManager = modManager;

            _updatePackageListCommand = ReactiveCommand.CreateAsyncTask(_ => UpdatePackageList());

            this.WhenAnyValue(x => x.ManagerStatusMessage).Select(val => !string.IsNullOrEmpty(val)).BindTo(this, x => x.HasManagerStatusMessage);

            CurrentState = InitializeFirstState();

            var setupObservable =
                bus.Listen<MainWindowOpenedMessage>()
                    .Select(x => Unit.Default)
                    .Delay(TimeSpan.FromSeconds(30))
                    .Merge(_activatedObservable)
                    .FirstAsync();
            setupObservable.InvokeCommand(_updatePackageListCommand);
        }

        private IModInstallationManager ModInstallationManager { get; set; }

        private IInstallationState InitializeFirstState()
        {
            return new PackageListViewModel(this, ModInstallationManager);
        }

        #region Overrides of Screen

        public override string DisplayName
        {
            get { return "Update/Install Mods"; }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            _activatedObservable.OnNext(Unit.Default);
        }

        #endregion

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

        public ICommand UpdatePackageListCommand
        {
            get { return _updatePackageListCommand; }
        }

        public IEnumerable<IModGroup<IModification>> ModGroups
        {
            get { return _modGroups; }
        }


        public async Task UpdatePackageList()
        {
            ManagerStatusMessage = "Parsing local mod information...";
            await ModInstallationManager.LocalModManager.ParseLocalModDataAsync();

            try
            {
                var exception = false;
                try
                {
                    await
                        ModInstallationManager.RemoteModManager.GetModGroupsAsync(new Progress<string>(msg => ManagerStatusMessage = msg),
                            true,
                            CancellationToken.None).ConfigureAwait(false);

                    await Observable.Start(() =>
                    {
                        _modGroups.Reset();
                        _modGroups.AddRange(ModInstallationManager.RemoteModManager.ModGroups);
                    },
                        RxApp.MainThreadScheduler);
                }
                catch (Exception e)
                {
                    this.Log().WarnException("Exception while updating package list!", e);
                    exception = true;
                }

                if (exception)
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
            }
            finally
            {
                ManagerStatusMessage = null;
            }
        }

        [NotNull, Import]
        private ITaskbarController TaskbarController { get; set; }

        public async Task ExecuteChanges(IEnumerable<InstallationItem> installations, IEnumerable<InstallationItem> uninstallations)
        {
            var installationsList = installations.ToList();
            var uninstallationsList = uninstallations.ToList();

            try
            {
                var operationOverview = new OperationsOverviewViewModel();
                operationOverview.InstallationItems = installationsList;
                operationOverview.UninstallationItems = uninstallationsList;

                CurrentState = operationOverview;

                var result = await operationOverview.GetResult();

                if (result != StateResult.Continue)
                {
                    return;
                }

                var installationVm = new InstallationViewModel();

                TaskbarController.ProgressbarVisible = true;
                TaskbarController.ProgressvarValue = 0.0;

                try
                {
                    installationVm.InstallationItems = installationsList;
                    installationVm.UninstallationItems = uninstallationsList;

                    CurrentState = installationVm;

                    using (installationVm.InstallationParent.WhenAnyValue(x => x.Progress).Subscribe(p =>
                    {
                        TaskbarController.ProgressvarValue = p;
                    }))
                    {
                        await
                            Task.WhenAll(installationVm.UninstallationParent.Install(), installationVm.InstallationParent.Install())
                                .ConfigureAwait(true);
                    }
                }
                finally
                {
                    TaskbarController.ProgressbarVisible = false;
                    TaskbarController.ProgressvarValue = 0.0;
                }

                await installationVm.GetResult();
            }
            finally
            {
                CurrentState = InitializeFirstState();
            }
        }

        #endregion
    }
}
