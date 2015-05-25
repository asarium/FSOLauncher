#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ReactiveUI;
using Splat;
using UI.WPF.Launcher.Common;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using ViewLocator = Caliburn.Micro.ViewLocator;

#endregion

namespace UI.WPF.Launcher.ViewModels
{
    [Export(typeof(IShellViewModel)), Export(typeof(IFlyoutManager))]
    public class ShellViewModel : Screen, IShellViewModel, IFlyoutManager
    {
        private bool _hasNoLauncherViewModel;

        private ILauncherViewModel _launcherViewModel;

        private bool _overlayVisible;

        private string _title = "FSO Launcher";

        [ImportingConstructor]
        public ShellViewModel(IMessageBus messageBus, ISettings settings, IInteractionService interactionService)
        {
            Settings = settings;

            messageBus.ListenIncludeLatest<InstanceLaunchedMessage>().Subscribe(HandleInstanceLaunched);

            RightCommands = new BindableCollection<UIElement>();
            WindowFlyouts = new BindableCollection<IFlyout>();

            this.WhenAnyValue(x => x.LauncherViewModel).Select(x => x == null).BindTo(this, x => x.HasNoLauncherViewModel);

            // Do the dependency resolution on a background thread so the UI can be shown as soon as possible
            var initializeCommand = ReactiveCommand.CreateAsyncTask(async _ => await InitializeAsync().ConfigureAwait(true));
            initializeCommand.ExecuteAsync().Subscribe();
        }

        public bool HasNoLauncherViewModel
        {
            get { return _hasNoLauncherViewModel; }
            private set
            {
                if (value.Equals(_hasNoLauncherViewModel))
                {
                    return;
                }
                _hasNoLauncherViewModel = value;
                NotifyOfPropertyChange();
            }
        }

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

        public IObservableCollection<IFlyout> WindowFlyouts { get; private set; }

        public ISettings Settings { get; private set; }

        #region IFlyoutManager Members

        #region Implementation of IFlyoutManager

        public void AddFlyout(IFlyout flyout)
        {
            if (!WindowFlyouts.Contains(flyout))
            {
                WindowFlyouts.Add(flyout);
            }
        }

        #endregion

        #endregion

        public void HandleInstanceLaunched(InstanceLaunchedMessage message)
        {
            var view = GetView() as Window;

            if (view != null)
            {
                view.Activate();
            }
        }
        
        #region IShellViewModel Members

        public bool OverlayVisible
        {
            get { return _overlayVisible; }
            set
            {
                if (value.Equals(_overlayVisible))
                {
                    return;
                }
                _overlayVisible = value;
                NotifyOfPropertyChange();
            }
        }

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

        public ILauncherViewModel LauncherViewModel
        {
            get { return _launcherViewModel; }
            private set
            {
                if (Equals(value, _launcherViewModel))
                {
                    return;
                }
                _launcherViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        private async Task InitializeAsync()
        {
            try
            {
                OverlayVisible = true;
                await Task.Yield();

                var settingsLoadTask = Settings.LoadAsync();

                var container = (CompositionContainer) Locator.Current.GetService(typeof(CompositionContainer));

                LauncherViewModel = await Task.Run(() => container.GetExportedValue<ILauncherViewModel>()).ConfigureAwait(true);

                if (LauncherViewModel == null)
                {
                    // Ehhhh
                    throw new InvalidOperationException("Something is very wrong! Please contact the developer!");
                }

                var tabs = await Task.Run(() => container.GetExports<ILauncherTab, ILauncherTabMetaData>().ToList()).ConfigureAwait(true);
                LauncherViewModel.LauncherTabs = tabs.OrderBy(x => x.Metadata.Priority).Select(x => x.Value);

                ImportedRightCommands =
                    await
                        Task.Run(() => container.GetExports<IWindowCommand, ILauncherTabMetaData>(ContractNames.RightWindowCommandsContract))
                            .ConfigureAwait(true);

                await settingsLoadTask.ConfigureAwait(true);

                // Now publish the message that we are alive
                Locator.Current.GetService<IMessageBus>().SendMessage(new MainWindowOpenedMessage());
            }
            finally
            {
                OverlayVisible = false;
            }
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                OverlayVisible = true;
                await Settings.SaveAsync().ConfigureAwait(true);
            }
            finally
            {
                OverlayVisible = false;
            }
        }
    }
}
