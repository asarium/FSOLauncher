#region Usings

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using UI.WPF.Launcher.Common;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.ViewModels
{
    [Export(ContractNames.RightWindowCommandsContract, typeof(IWindowCommand)), ExportMetadata("Priority", 0)]
    public sealed class UpdateViewModel : Screen, IWindowCommand, IPartImportsSatisfiedNotification
    {
        public UpdateViewModel()
        {
            DisplayName = "Update";
        }

        private object _status;

        [Import]
        public IUpdateService UpdateService { private get; set; }

        [Import]
        public ISettings Settings { private get; set; }

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

        #region IPartImportsSatisfiedNotification Members

        public async void OnImportsSatisfied()
        {
            await StartUpdateCheck();
        }

        #endregion

        public async Task StartUpdateCheck()
        {
            if (!UpdateService.IsUpdatePossible)
            {
                Status = new ErrorStatus("This application was not deployed correctly.");
                return;
            }

            if (Settings != null && !Settings.CheckForUpdates)
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

        public async Task DoUpdate(UpdateVersion version)
        {
            // Update available
            Status = new UpdatingStatus();
            try
            {
                await UpdateService.DoUpdateAsync(new Progress<IUpdateProgress>(((UpdatingStatus) Status).UpdateProgress));
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
        }
    }
}
