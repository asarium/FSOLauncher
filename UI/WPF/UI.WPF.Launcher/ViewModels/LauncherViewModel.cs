#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles.DataClass;
using ModInstallation.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Views;
using UI.WPF.Modules.Implementations.Implementations;

#endregion

namespace UI.WPF.Launcher.ViewModels
{
    [Export(typeof(ILauncherViewModel))]
    public class LauncherViewModel : Conductor<ILauncherTab>.Collection.OneActive, ILauncherViewModel
    {
        private readonly ReactiveList<IModRepositoryViewModel> _repositories;

        private readonly ReactiveList<TotalConversion> _totalConversions;

        private ICommand _addGameRootCommand;

        private ICommand _addProfileCommand;

        private bool _hasTotalConversions;

        private ICommand _launchGameCommand;

        [ImportingConstructor]
        public LauncherViewModel([NotNull] ISettings settings,
            [NotNull] IRepositoryFactory factory,
            IModInstallationManager modInstallationManager,
            IProfileManager profileManager, IMessageBus bus)
        {
            ScreenExtensions.TryActivate(this);

            ModInstallationManager = modInstallationManager;
            ProfileManager = profileManager;

            _totalConversions = new ReactiveList<TotalConversion>();
            _repositories = new ReactiveList<IModRepositoryViewModel>();

            AddGameRootCommand = ReactiveCommand.CreateAsyncTask(_ => AddGameRoot());

            AddProfileCommand = ReactiveCommand.CreateAsyncTask(_ => AddProfile());

            settings.WhenAnyValue(x => x.SelectedProfile).Subscribe(profile =>
            {
                if (profile == null)
                {
                    LaunchGameCommand = null;
                }
                else
                {
                    LaunchGameCommand = ReactiveCommand.CreateAsyncTask(profile.CanLaunchExecutable,
                        async _ => await LaunchExecutable(settings.SelectedProfile).ConfigureAwait(true));
                }
            });

            HasTotalConversions = _totalConversions.Count > 0;
            _totalConversions.CountChanged.Select(val => val > 0).BindTo(this, x => x.HasTotalConversions);

            ProfileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion.RootFolder).BindTo(ModInstallationManager, x => x.RootPath);

            bus.Listen<MainWindowOpenedMessage>().Subscribe(_ =>
            {
                using (_totalConversions.SuppressChangeNotifications())
                {
                    _totalConversions.Clear();
                    _totalConversions.AddRange(settings.TotalConversions);
                }

                using (_repositories.SuppressChangeNotifications())
                {
                    _repositories.Clear();

                    if (settings.ModRepositories == null)
                    {
                        _repositories.Add(new ModRepositoryViewModel
                        {
                            Name = "Default",
                            Repository = factory.ConstructRepository("https://fsnebula.org/repo/test.json")
                        });
                    }
                    else
                    {
                        _repositories.AddRange(settings.ModRepositories);
                    }
                }
            });

            _totalConversions.Changed.Subscribe(_ => settings.TotalConversions = _totalConversions);
            _repositories.Changed.Subscribe(_ =>
            {
                settings.ModRepositories = _repositories;
                ModInstallationManager.RemoteModManager.Repositories = _repositories.Select(x => x.Repository);
            });
        }

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

        [Import]
        private IInteractionService InteractionService { get; set; }

        #region ILauncherViewModel Members

        public IReactiveList<TotalConversion> TotalConversions
        {
            get { return _totalConversions; }
        }

        public IReactiveList<IModRepositoryViewModel> ModRepositories
        {
            get { return _repositories; }
        }

        public IEnumerable<ILauncherTab> LauncherTabs
        {
            set
            {
                Items.Clear();
                Items.AddRange(value);

                if (Items.Any())
                {
                    ActivateItem(Items.First());
                }
            }
        }

        public IModInstallationManager ModInstallationManager { get; private set; }

        public IProfileManager ProfileManager { get; private set; }

        public ICommand AddProfileCommand
        {
            get { return _addProfileCommand; }
            private set
            {
                if (Equals(value, _addProfileCommand))
                {
                    return;
                }
                _addProfileCommand = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand LaunchGameCommand
        {
            get { return _launchGameCommand; }
            private set
            {
                if (Equals(value, _launchGameCommand))
                {
                    return;
                }
                _launchGameCommand = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand AddGameRootCommand
        {
            get { return _addGameRootCommand; }
            private set
            {
                if (Equals(value, _addGameRootCommand))
                {
                    return;
                }
                _addGameRootCommand = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        private async Task AddProfile()
        {
            var dialog = new ProfileInputDialog(ProfileManager)
            {
                Title = "Add a new profile"
            };

            var result = await InteractionService.ShowDialog(dialog).ConfigureAwait(true);

            if (result.Cancelled)
            {
                return;
            }

            IProfile profile;
            if (result.ClonedProfile != null)
            {
                var cloneProfile = ProfileManager.Profiles.First(p => p.Name == result.ClonedProfile);

                profile = (IProfile) cloneProfile.Clone();
                profile.Name = result.Name;
            }
            else
            {
                profile = ProfileManager.CreateNewProfile(result.Name);
                await profile.PullConfigurationAsync(CancellationToken.None).ConfigureAwait(true);
            }

            ProfileManager.AddProfile(profile);
            ProfileManager.CurrentProfile = profile;
        }

        private static async Task LaunchExecutable(IProfile selectedProfile)
        {
            if (selectedProfile == null)
            {
                return;
            }

            await selectedProfile.LaunchSelectedExecutableAsync(CancellationToken.None, new Progress<string>()).ConfigureAwait(true);
        }

        private async Task AddGameRoot()
        {
            var dialog = new GameRootInputDialog(TotalConversions)
            {
                Title = "Add a new game directory"
            };

            var result = await InteractionService.ShowDialog(dialog).ConfigureAwait(true);

            if (result.SelectedButton == RootDialogResult.Button.Canceled)
            {
                return;
            }

            var tc = new TotalConversion();
            tc.InitializeFromData(new TcData
            {
                Name = result.Name,
                RootPath = result.Path
            });

            TotalConversions.Add(tc);

            if (ProfileManager.CurrentProfile != null)
            {
                ProfileManager.CurrentProfile.SelectedTotalConversion = tc;
            }
        }
    }
}
