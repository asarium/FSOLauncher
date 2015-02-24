#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement.Annotations;
using ReactiveUI;
using UI.WPF.Launcher.Common;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.Update.Views;

#endregion

namespace UI.WPF.Modules.Update.ViewModels
{
    [Export(ContractNames.RightWindowCommandsContract, typeof(IWindowCommand)), ExportMetadata("Priority", 0)]
    public sealed class UpdateViewModel : Screen, IWindowCommand, IHandle<MainWindowOpenedMessage>
    {
        private object _status;

        private readonly ReactiveCommand<Unit> _checkForUpdatesCommand;

        [ImportingConstructor]
        public UpdateViewModel([NotNull] IEventAggregator aggregator)
        {
            DisplayName = "Update";

            aggregator.Subscribe(this);

            _checkForUpdatesCommand = ReactiveCommand.CreateAsyncTask((_, token) => StartUpdateCheck());
        }

        [NotNull, Import]
        public IUpdateService UpdateService { private get; set; }

        [NotNull, Import]
        public ISettings Settings { private get; set; }

        [NotNull, Import]
        public IInteractionService InteractionService { private get; set; }

        [NotNull]
        public ICommand CheckForUpdatesCommand
        {
            get { return _checkForUpdatesCommand; }
        }

        public object Status
        {
            get { return _status; }
            set
            {
                if (Equals(value, _status))
                {
                    return;
                }
                _status = value;
                NotifyOfPropertyChange();
            }
        }

        #region IHandle<MainWindowOpenedMessage> Members

        void IHandle<MainWindowOpenedMessage>.Handle([NotNull] MainWindowOpenedMessage message)
        {
            // Delay checking for a bit...
            Observable.Timer(TimeSpan.FromSeconds(5)).InvokeCommand(_checkForUpdatesCommand);
        }

        #endregion

        [NotNull]
        public async Task StartUpdateCheck()
        {
            if (!UpdateService.IsUpdatePossible)
            {
                Status = new ErrorStatus("This application was not deployed correctly.");
                return;
            }

            if (!Settings.CheckForUpdates)
            {
                Status = new SuccessfullStatus("Not checking for updates.");
                return;
            }

            Status = new UpdateCheckStatus();

            IUpdateStatus state;

            try
            {
                state = await UpdateService.CheckForUpdateAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Status = new ErrorStatus(string.Format("Failed to check for update:\n{0}", e.Message));
                return;
            }

            if (state.UpdateAvailable)
            {
                await DoUpdate(state.Version).ConfigureAwait(false);
            }
            else
            {
                Status = new SuccessfullStatus("You are using the latest version.");
            }
        }

        [NotNull]
        private async Task DoUpdate([CanBeNull] Version version)
        {
            // Update available
            Status = new UpdatingStatus();
            IUpdateProgress last = null;
            try
            {
                await UpdateService.DoUpdateAsync(new Progress<IUpdateProgress>(progress =>
                {
                    last = progress;
                    ((UpdatingStatus) Status).UpdateProgress(progress);
                })).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Status = new ErrorStatus(string.Format("Failed to update:\n{0}", e.Message));
                return;
            }

            if (last.State == UpdateState.Finished && last.ReleaseNotes != null)
            {
                Status = new ChangeLogStatus(last.ReleaseNotes, InteractionService);
            }
            else if (version != null)
            {
                Status = new SuccessfullStatus(string.Format("Update to version {0} was successfull.", version));
            }
            else
            {
                Status = new SuccessfullStatus("Update was successfull.");
            }
        }
    }
}
