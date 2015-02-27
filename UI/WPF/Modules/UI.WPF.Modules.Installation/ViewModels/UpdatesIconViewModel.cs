#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    [Export(ContractNames.RightWindowCommandsContract, typeof(IWindowCommand)), ExportMetadata("Priority", 1)]
    public class UpdatesIconViewModel : Screen, IWindowCommand, IHandle<MainWindowOpenedMessage>
    {
        private readonly IReactiveCommand<Unit> _checkForUpdatesCommand;

        private bool _checkingForUpdates;

        private string _currentMessage;

        private bool _isVisible;

        private int _numberOfUpdates;

        private IEnumerable<IModification> _updatedMods;

        private bool _updatesAvailable;

        [ImportingConstructor]
        public UpdatesIconViewModel([NotNull] IEventAggregator aggregator, IRemoteModManager remoteManager, ILocalModManager localModManager)
        {
            RemoteManager = remoteManager;
            LocalModManager = localModManager;
            aggregator.Subscribe(this);

            _checkForUpdatesCommand = ReactiveCommand.CreateAsyncTask((_, tok) => CheckForModUpdates());

            this.WhenAnyValue(x => x.CheckingForUpdates, x => x.UpdatesAvailable).Select(x => x.Item1 || x.Item2).BindTo(this, x => x.IsVisible);
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (value.Equals(_isVisible))
                {
                    return;
                }
                _isVisible = value;
                NotifyOfPropertyChange();
            }
        }

        private IRemoteModManager RemoteManager { get; set; }

        private ILocalModManager LocalModManager { get; set; }

        public string CurrentMessage
        {
            get { return _currentMessage; }
            private set
            {
                if (value == _currentMessage)
                {
                    return;
                }
                _currentMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool UpdatesAvailable
        {
            get { return _updatesAvailable; }
            private set
            {
                if (value.Equals(_updatesAvailable))
                {
                    return;
                }
                _updatesAvailable = value;
                NotifyOfPropertyChange();
            }
        }

        public int NumberOfUpdates
        {
            get { return _numberOfUpdates; }
            private set
            {
                if (value == _numberOfUpdates)
                {
                    return;
                }
                _numberOfUpdates = value;
                NotifyOfPropertyChange();
            }
        }

        public IEnumerable<IModification> UpdatedMods
        {
            get { return _updatedMods; }
            private set
            {
                if (Equals(value, _updatedMods))
                {
                    return;
                }
                _updatedMods = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CheckingForUpdates
        {
            get { return _checkingForUpdates; }
            private set
            {
                if (value.Equals(_checkingForUpdates))
                {
                    return;
                }
                _checkingForUpdates = value;
                NotifyOfPropertyChange();
            }
        }

        #region IHandle<MainWindowOpenedMessage> Members

        void IHandle<MainWindowOpenedMessage>.Handle([NotNull] MainWindowOpenedMessage message)
        {
            // Delay checking for a bit...
            Observable.Timer(TimeSpan.FromSeconds(10)).InvokeCommand(_checkForUpdatesCommand);
        }

        #endregion

        private async Task CheckForModUpdates()
        {
            CheckingForUpdates = true;

            try
            {
                var localMods = LocalModManager.Modifications;
                if (localMods == null)
                {
                    await LocalModManager.ParseLocalModDataAsync().ConfigureAwait(false);
                    localMods = LocalModManager.Modifications;
                }

                var localModList = localMods as IList<IInstalledModification> ?? localMods.ToList();

                if (localMods == null || !localModList.Any())
                {
                    UpdatesAvailable = false;
                    return;
                }

                var remoteGroups =
                    await
                        RemoteManager.GetModGroupsAsync(new Progress<string>(msg => CurrentMessage = msg), false, CancellationToken.None)
                            .ConfigureAwait(false);
                CurrentMessage = null;

                // Now check the remote mods and the local mods, do it in task to not block the UI
                var updates = await Task.Run(() => remoteGroups.Select(g => new
                {
                    Group = g,
                    Local = localModList.Where(m => string.Equals(m.Id, g.Id))
                })
                    .Where(x => x.Local.Any())
                    .Select(x => x.Group.Versions.Where(p => x.Local.All(m => p.Key > m.Version)).OrderByDescending(p => p.Key).Select(p => p.Value))
                    .Select(u => u.FirstOrDefault())
                    .Where(x => x != null)
                    .ToList()).ConfigureAwait(false);

                UpdatesAvailable = updates.Count > 0;
                if (UpdatesAvailable)
                {
                    NumberOfUpdates = updates.Count;
                    UpdatedMods = updates;

                    CurrentMessage = GetUpdatesMessage();
                }
                else
                {
                    NumberOfUpdates = -1;
                    UpdatedMods = null;
                }
            }
            finally
            {
                CheckingForUpdates = false;
            }
        }

        private string GetUpdatesMessage()
        {

            var builder = new StringBuilder();

            if (NumberOfUpdates == 1)
            {
                // Special message if only one update is available
                builder.AppendFormat("There is an update for the mod {0} available.", UpdatedMods.First().Title);
            }
            else
            {

                builder.AppendFormat("There are updates available for the following mods:");

                foreach (var modification in UpdatedMods)
                {
                    builder.AppendFormat("\n   {0}", modification.Title);
                }
            }

            builder.Append("\nClick here to install the updates.");

            return builder.ToString();
        }
    }
}
