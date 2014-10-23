#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using FSOManagement.Annotations;
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

        [ImportingConstructor]
        public UpdateViewModel([NotNull] IEventAggregator aggregator)
        {
            DisplayName = "Update";

            aggregator.Subscribe(this);
        }

        [NotNull,Import]
        public IUpdateService UpdateService { private get; set; }

        [NotNull,Import]
        public ISettings Settings { private get; set; }

        [NotNull,Import]
        public IInteractionService InteractionService { private get; set; }

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
            StartUpdateCheck();
        }

        #endregion

        public async void StartUpdateCheck()
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
                state = await UpdateService.CheckForUpdateAsync();
            }
            catch (Exception e)
            {
                Status = new ErrorStatus(string.Format("Failed to check for update:\n{0}", e.Message));
                return;
            }

            if (state.UpdateAvailable)
            {
                await DoUpdate(state.Version);
            }
            else
            {
                Status = new SuccessfullStatus("You are using the latest version.");
            }
        }

        [NotNull]
        public async Task DoUpdate([CanBeNull] Version version)
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
                }));
            }
            catch (Exception e)
            {
                Status = new ErrorStatus(string.Format("Failed to update:\n{0}", e.Message));
                return;
            }

            if (version != null)
            {
                Status = new SuccessfullStatus(string.Format("Update to version {0} was successfull.", version));
            }
            else
            {
                Status = new SuccessfullStatus("Update was successfull.");
            }

            if (last.State == UpdateState.Finished)
            {
                OpenChangelogDialog(last);
            }
        }

        private void OpenChangelogDialog([NotNull] IUpdateProgress last)
        {
            if (last.ReleaseNotes == null)
            {
                return;
            }

            var dialog = new ChangelogDialog(last.ReleaseNotes);

            InteractionService.ShowDialog(dialog);
        }
    }
}
