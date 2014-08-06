#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement.Interfaces;
using ReactiveUI;
using SDLGlue;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    [Export(typeof(VideoSettingsViewModel))]
    public class VideoSettingsViewModel : PropertyChangedBase
    {
        private static readonly DisplayModeComparer DisplayModeComparer = new DisplayModeComparer();

        private IFlagManager _flagManager;

        private ListCollectionView _resolutionCollectionView;

        private string _selectedTextureFilter;

        private VideoDisplayViewModel _selectedVideoDisplay;

        private WindowModeViewModel _selectedWindowMode;

        [ImportingConstructor]
        public VideoSettingsViewModel(IProfileManager profileManager)
        {
            InitializeResolutions();

            var detectCommand = ReactiveCommand.Create();
            detectCommand.Subscribe(_ => DetectResolution(true));

            DetectResolutionCommand = detectCommand;

            InitializeVideoDisplaySelection(profileManager);

            InitializeWindowMode(profileManager);

            InitializeTextureFilter(profileManager);
        }

        public IEnumerable<WindowModeViewModel> WindowModes
        {
            get
            {
                var values = Enum.GetValues(typeof(WindowModeViewModel.WindowingType)).Cast<WindowModeViewModel.WindowingType>();

                return values.Select(windowingType => new WindowModeViewModel(windowingType));
            }
        }

        public WindowModeViewModel SelectedWindowMode
        {
            get { return _selectedWindowMode; }
            set
            {
                if (Equals(value, _selectedWindowMode))
                {
                    return;
                }
                _selectedWindowMode = value;
                NotifyOfPropertyChange();
            }
        }

        [Import]
        private IInteractionService InteractionService { get; set; }

        public ListCollectionView ResolutionCollectionView
        {
            get { return _resolutionCollectionView; }
            set
            {
                if (Equals(value, _resolutionCollectionView))
                {
                    return;
                }
                _resolutionCollectionView = value;
                NotifyOfPropertyChange();
            }
        }

        public VideoDisplayViewModel SelectedVideoDisplay
        {
            get { return _selectedVideoDisplay; }
            set
            {
                if (Equals(value, _selectedVideoDisplay))
                {
                    return;
                }
                _selectedVideoDisplay = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand DetectResolutionCommand { get; private set; }

        public IFlagManager FlagManager
        {
            get { return _flagManager; }
            private set
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

        public IEnumerable<string> TextureFilters
        {
            get { return Enum.GetNames(typeof(TextureFiltering)); }
        }

        public string SelectedTextureFilter
        {
            get { return _selectedTextureFilter; }
            set
            {
                if (value == _selectedTextureFilter)
                {
                    return;
                }
                _selectedTextureFilter = value;
                NotifyOfPropertyChange();
            }
        }

        private void InitializeTextureFilter(IProfileManager profileManager)
        {
            // Not really a clean solution but this solves the infinite recursion problem
            var notifying = false;

            var settingsObservable = profileManager.WhenAny(x => x.CurrentProfile.TextureFiltering,
                val => Enum.GetName(typeof(TextureFiltering), val.Value));

            var thisObservable = this.WhenAny(x => x.SelectedTextureFilter, val =>
            {
                TextureFiltering value;

                return Enum.TryParse(val.Value, true, out value) ? value : TextureFiltering.Trilinear;
            });

            settingsObservable.Subscribe(filter =>
            {
                if (notifying)
                {
                    return;
                }

                notifying = true;
                SelectedTextureFilter = filter;
                notifying = false;
            });

            thisObservable.Subscribe(filter =>
            {
                if (notifying)
                {
                    return;
                }

                notifying = true;
                profileManager.CurrentProfile.TextureFiltering = filter;
                notifying = false;
            });
        }

        private void FlagManagerOnFlagChanged(object sender, FlagChangedEventArgs args)
        {
            UpdateWindowMode((IFlagManager) sender);
        }

        private void UpdateWindowMode(IFlagManager manager)
        {
            if (manager.IsFlagSet("-fullscreen_window"))
            {
                SelectedWindowMode = new WindowModeViewModel(WindowModeViewModel.WindowingType.Borderless);
            }
            else if (manager.IsFlagSet("-window"))
            {
                SelectedWindowMode = new WindowModeViewModel(WindowModeViewModel.WindowingType.Windowed);
            }
            else
            {
                SelectedWindowMode = new WindowModeViewModel(WindowModeViewModel.WindowingType.Fullscreen);
            }
        }

        private void InitializeWindowMode(IProfileManager profileManager)
        {
            profileManager.WhenAnyValue(x => x.CurrentProfile.FlagManager).BindTo(this, x => x.FlagManager);

            this.WhenAnyValue(x => x.FlagManager).Subscribe(UpdateWindowMode);

            this.WhenAnyValue(x => x.SelectedWindowMode).Subscribe(mode =>
            {
                switch (mode.Value)
                {
                    case WindowModeViewModel.WindowingType.Fullscreen:
                        profileManager.CurrentProfile.FlagManager.SetFlag("-window", false);
                        profileManager.CurrentProfile.FlagManager.SetFlag("-fullscreen_window", false);
                        break;
                    case WindowModeViewModel.WindowingType.Borderless:
                        profileManager.CurrentProfile.FlagManager.SetFlag("-window", false);
                        profileManager.CurrentProfile.FlagManager.SetFlag("-fullscreen_window", true);
                        break;
                    case WindowModeViewModel.WindowingType.Windowed:
                        profileManager.CurrentProfile.FlagManager.SetFlag("-window", true);
                        profileManager.CurrentProfile.FlagManager.SetFlag("-fullscreen_window", false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private void InitializeVideoDisplaySelection(IProfileManager profileManager)
        {
            SelectedVideoDisplay =
                ResolutionCollectionView.OfType<VideoDisplayViewModel>()
                    .FirstOrDefault(
                        display =>
                            display.Width == profileManager.CurrentProfile.ResolutionWidth &&
                            display.Height == profileManager.CurrentProfile.ResolutionHeight);

            if (SelectedVideoDisplay == null)
            {
                DetectResolution(false);
            }

            this.WhenAny(x => x.SelectedVideoDisplay, val => val.Value.Width).BindTo(profileManager, x => x.CurrentProfile.ResolutionWidth);
            this.WhenAny(x => x.SelectedVideoDisplay, val => val.Value.Height).BindTo(profileManager, x => x.CurrentProfile.ResolutionHeight);
        }

        private void InitializeResolutions()
        {
            var displays = SDLVideoDisplay.GetVideoDisplays();

            var videoDisplayCollection =
                displays.First().DisplayModes.Distinct(DisplayModeComparer).CreateDerivedCollection(mode => new VideoDisplayViewModel(mode));

            ResolutionCollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(videoDisplayCollection);
            ResolutionCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("AspectRatio"));
        }

        private void DetectResolution(bool interactive)
        {
            var desktopModes = SDLVideoDisplay.DesktopVideoModes.ToArray();

            if (!desktopModes.Any())
            {
                if (interactive)
                {
                    InteractionService.ShowMessage(MessageType.Error, "SDL error", "Could not determine the size of your current display!");
                }
                return;
            }

            var firstDisplay = desktopModes.First();
            var foundResolution =
                ResolutionCollectionView.OfType<VideoDisplayViewModel>()
                    .FirstOrDefault(display => display.Width == firstDisplay.Width && display.Height == firstDisplay.Height);

            if (foundResolution == null)
            {
                if (interactive)
                {
                    InteractionService.ShowMessage(MessageType.Information, "No resolution found",
                        "No exactly matching resolution could be found, sorry...");
                }
            }
            else
            {
                SelectedVideoDisplay = foundResolution;
            }

            SelectedWindowMode =
                new WindowModeViewModel(desktopModes.Length > 1
                    ? WindowModeViewModel.WindowingType.Borderless
                    : WindowModeViewModel.WindowingType.Windowed);
        }
    }
}
