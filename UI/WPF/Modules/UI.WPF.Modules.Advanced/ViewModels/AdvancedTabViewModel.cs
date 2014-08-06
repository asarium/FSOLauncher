#region Usings

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Advanced.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 2)]
    public sealed class AdvancedTabViewModel : Screen, ILauncherTab
    {
        private BuildCapabilities _currentBuildCaps;

        private ListCollectionView _flagCollectionView;

        private IFlagManager _flagManager;

        private bool _hasExecutable;

        private string _currentCommandLine;

        [ImportingConstructor]
        public AdvancedTabViewModel(IProfileManager profileManager)
        {
            DisplayName = "Advanced";

            ProfileManager = profileManager;

            profileManager.WhenAny(x => x.CurrentProfile.SelectedExecutable, exe => exe.Value != null).BindTo(this, x => x.HasExecutable);

            var executableObservable = profileManager.WhenAnyValue(x => x.CurrentProfile.SelectedExecutable);

            var regenerateCommand = ReactiveCommand.CreateAsyncTask(async (arg, token) => await RegenerateFlagList(token));
            executableObservable.InvokeCommand(regenerateCommand);

            // When the profile changes regenerate the list to make sure the references are right
            profileManager.WhenAnyValue(x => x.CurrentProfile).InvokeCommand(regenerateCommand);

            // Bind the flag manager of the the current profile
            profileManager.WhenAnyValue(x => x.CurrentProfile.FlagManager).BindTo(this, x => x.FlagManager);

            // Bind buildCaps -> FlagCollectionView
            this.WhenAny(x => x.CurrentBuildCaps, val => GenerateFlagView(CreateViewModelEnumerable(val.Value)))
                .BindTo(this, x => x.FlagCollectionView);

            profileManager.WhenAnyValue(x => x.CurrentProfile.CommandLine).BindTo(this, x => x.CurrentCommandLine);

            RegenerateListCommand = regenerateCommand;
        }

        public string CurrentCommandLine
        {
            get { return _currentCommandLine; }
            private set
            {
                if (value == _currentCommandLine)
                {
                    return;
                }
                _currentCommandLine = value;
                NotifyOfPropertyChange();
            }
        }

        private IProfileManager ProfileManager { get; set; }

        private IFlagManager FlagManager
        {
            get { return _flagManager; }
            set
            {
                if (Equals(value, _flagManager))
                {
                    return;
                }

                if (_flagManager != null)
                {
                    _flagManager.FlagChanged -= FlagManagerOnFlagChanged;
                }

                _flagManager = value;

                if (_flagManager != null)
                {
                    _flagManager.FlagChanged += FlagManagerOnFlagChanged;
                }

                NotifyOfPropertyChange();
            }
        }

        [Import]
        private IInteractionService InteractionService { get; set; }

        public ListCollectionView FlagCollectionView
        {
            get { return _flagCollectionView; }
            private set
            {
                if (Equals(value, _flagCollectionView))
                {
                    return;
                }
                _flagCollectionView = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasExecutable
        {
            get { return _hasExecutable; }
            private set
            {
                if (value.Equals(_hasExecutable))
                {
                    return;
                }
                _hasExecutable = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand RegenerateListCommand { get; private set; }

        public BuildCapabilities CurrentBuildCaps
        {
            get { return _currentBuildCaps; }
            private set
            {
                if (Equals(value, _currentBuildCaps))
                {
                    return;
                }
                _currentBuildCaps = value;
                NotifyOfPropertyChange();
            }
        }

        private IEnumerable<FlagViewModel> CreateViewModelEnumerable(BuildCapabilities val)
        {
            if (val == null)
            {
                return null;
            }

            return val.Flags.Select(flag =>
            {
                var viewModel = new FlagViewModel(flag, ProfileManager.CurrentProfile.FlagManager);
                viewModel.SetEnabled(ProfileManager.CurrentProfile.FlagManager.IsFlagSet(flag.Name));
                return viewModel;
            });
        }

        private void FlagManagerOnFlagChanged(object sender, FlagChangedEventArgs args)
        {
            foreach (var viewModel in FlagCollectionView.Cast<FlagViewModel>().Where(model => model.Flag.Name == args.Name))
            {
                viewModel.SetEnabled(args.Enabled);
            }
        }

        private async Task RegenerateFlagList(CancellationToken token)
        {
            if (ProfileManager.CurrentProfile.SelectedExecutable == null)
            {
                // Clear list
                CurrentBuildCaps = null;
            }
            else
            {
                // Regenerate list
                var selectedExecutable = ProfileManager.CurrentProfile.SelectedExecutable;
                CurrentBuildCaps = await selectedExecutable.GetBuildCapabilitiesAsync(token);

                if (CurrentBuildCaps == null)
                {
                    await
                        InteractionService.ShowMessage(MessageType.Error, "FreeSpace error",
                            "FreeSpace has not generated a flag file required to determine the supported features!");
                }
            }
        }

        private static ListCollectionView GenerateFlagView(IEnumerable<FlagViewModel> flagViewModels)
        {
            if (flagViewModels == null)
            {
                return null;
            }

            var view = (ListCollectionView) CollectionViewSource.GetDefaultView(flagViewModels.ToList());
            // Need to use ToList here to get a list collectionview...

            if (view.GroupDescriptions != null)
            {
                view.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            }

            return view;
        }
    }
}
