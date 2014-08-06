#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Launcher.Views;
using ViewLocator = Caliburn.Micro.ViewLocator;

#endregion

namespace UI.WPF.Launcher.ViewModels
{
    [Export(typeof(IShellViewModel))]
    public class ShellViewModel : Conductor<ILauncherTab>.Collection.OneActive, IShellViewModel, IHandle<InstanceLaunchedMessage>
    {
        private bool _hasTotalConversions;

        private ILauncherViewModel _launcherViewModel;

        private string _title = "FSO Launcher";

        [ImportingConstructor]
        public ShellViewModel(IEventAggregator eventAggregator, ISettings settings)
        {
            AddGameRootCommand = ReactiveCommand.CreateAsyncTask(async _ => await AddGameRoot());

            var canLaunchObservable = settings.WhenAnyValue(x => x.SelectedProfile.CanLaunchExecutable);
            LaunchGameCommand = ReactiveCommand.CreateAsyncTask(canLaunchObservable, async _ => await LaunchExecutable(settings.SelectedProfile));

            Settings = settings;

            eventAggregator.Subscribe(this);

            RightCommands = new BindableCollection<UIElement>();
        }

        public ICommand LaunchGameCommand { get; private set; }

        public ICommand AddGameRootCommand { get; private set; }

        [Import]
        private IInteractionService InteractionService { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ILauncherTab, ILauncherTabMetaData>> ImportedTabs
        {
            set
            {
                var orderedTabs = value.OrderBy(val => val.Metadata.Priority).Select(val => val.Value);

                Items.Clear();
                Items.AddRange(orderedTabs);

                if (Items.Any())
                {
                    ActivateItem(Items.First());
                }
            }
        }

        [ImportMany(ContractNames.RightWindowCommandsContract)]
        private IEnumerable<Lazy<IWindowCommand, ILauncherTabMetaData>> ImportedRightCommands
        {
            set
            {
                var commands = value.OrderBy(val => val.Metadata.Priority).Select(val => val.Value).Select(cmd =>
                {
                    var view = ViewLocator.LocateForModel(cmd, null, null);
                    var frameworkElement = view as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        frameworkElement.DataContext = cmd;
                    }

                    return view;
                });

                RightCommands.Clear();
                RightCommands.AddRange(commands);
            }
        }

        public IObservableCollection<UIElement> RightCommands { get; private set; }

        public bool HasTotalConversions
        {
            get { return _hasTotalConversions; }
            private set
            {
                if (value.Equals(_hasTotalConversions))
                {
                    return;
                }
                _hasTotalConversions = value;
                NotifyOfPropertyChange();
            }
        }

        public ISettings Settings { get; private set; }

        #region IHandle<InstanceLaunchedMessage> Members

        void IHandle<InstanceLaunchedMessage>.Handle(InstanceLaunchedMessage message)
        {
            var view = GetView() as Window;

            if (view != null)
            {
                view.Activate();
            }
        }

        #endregion

        #region IShellViewModel Members

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                {
                    return;
                }
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        [Import]
        public ILauncherViewModel LauncherViewModel
        {
            get { return _launcherViewModel; }
            private set
            {
                _launcherViewModel = value;

                var tcs = LauncherViewModel.TotalConversions;

                tcs.CountChanged.Select(val => val > 0).BindTo(this, x => x.HasTotalConversions);
                HasTotalConversions = tcs.Count > 0;
            }
        }

        #endregion

        private static async Task LaunchExecutable(IProfile selectedProfile)
        {
            if (selectedProfile == null)
            {
                return;
            }

            await selectedProfile.LaunchSelectedExecutableAsync(CancellationToken.None, new Progress<string>());
        }

        private async Task AddGameRoot()
        {
            var dialog = new GameRootInputDialog {Title = "Add a new game directory"};

            var result = await InteractionService.ShowDialog(dialog);

            if (result.SelectedButton == RootDialogResult.Button.Canceled)
            {
                return;
            }

            var tc = new TotalConversion(result.Name, result.Path);
            LauncherViewModel.TotalConversions.Add(tc);
            LauncherViewModel.ProfileManager.CurrentProfile.SelectedTotalConversion = tc;
        }
    }
}
