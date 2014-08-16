#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Windows.Input;
using FSOManagement.Interfaces;
using ReactiveUI;
using SDLGlue;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Launcher.Common.Util;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    public class VideoSettingsViewModel : ReactiveObject
    {
        private static readonly DisplayModeComparer DisplayModeComparer = new DisplayModeComparer();

        private IFlagManager _flagManager;

        private ListCollectionView _resolutionCollectionView;

        private string _selectedTextureFilter;

        private VideoDisplayViewModel _selectedVideoDisplay;

        private WindowModeViewModel _selectedWindowMode;

        public VideoSettingsViewModel(IProfile profile)
        {
            InitializeResolutions();

            var detectCommand = ReactiveCommand.Create();
            detectCommand.Subscribe(_ => DetectResolution(true));

            DetectResolutionCommand = detectCommand;

            InitializeVideoDisplaySelection(profile);

            InitializeWindowMode(profile);

            InitializeTextureFilter(profile);
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
            set { this.RaiseAndSetIfChanged(ref _selectedWindowMode, value); }
        }

        [Import]
        private IInteractionService InteractionService { get; set; }

        public ListCollectionView ResolutionCollectionView
        {
            get { return _resolutionCollectionView; }
            set { this.RaiseAndSetIfChanged(ref _resolutionCollectionView, value); }
        }

        public VideoDisplayViewModel SelectedVideoDisplay
        {
            get { return _selectedVideoDisplay; }
            set { this.RaiseAndSetIfChanged(ref _selectedVideoDisplay, value); }
        }

        public ICommand DetectResolutionCommand { get; private set; }

        public IFlagManager FlagManager
        {
            get { return _flagManager; }
            private set { this.RaiseAndSetIfChanged(ref _flagManager, value); }
        }

        public IEnumerable<string> TextureFilters
        {
            get { return Enum.GetNames(typeof(TextureFiltering)); }
        }

        public string SelectedTextureFilter
        {
            get { return _selectedTextureFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedTextureFilter, value); }
        }

        private void InitializeTextureFilter(IProfile profile)
        {
            profile.WhenAny(x => x.TextureFiltering, val => Enum.GetName(typeof(TextureFiltering), val.Value))
                .BindTo(this, x => x.SelectedTextureFilter);

            this.WhenAny(x => x.SelectedTextureFilter, val =>
            {
                TextureFiltering value;

                return Enum.TryParse(val.Value, true, out value) ? value : TextureFiltering.Trilinear;
            }).BindTo(profile, x => x.TextureFiltering);
        }

        private void FlagManagerOnFlagChanged(object sender, FlagChangedEventArgs args)
        {
            UpdateWindowMode((IFlagManager) sender);
        }

        private void UpdateWindowMode(IFlagManager manager)
        {
            if (manager == null)
            {
                return;
            }

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

        private void InitializeWindowMode(IProfile profile)
        {
            profile.WhenAnyValue(x => x.FlagManager).BindTo(this, x => x.FlagManager);

            this.WhenAnyValue(x => x.FlagManager).Subscribe(UpdateWindowMode);

            var beforeChange = this.ObservableForProperty(x => x.FlagManager, true).Select(x => x.Value);

            var afterChange = this.ObservableForProperty(x => x.FlagManager).Select(x => x.Value);

            beforeChange.Subscribe(manager =>
            {
                if (manager != null)
                {
                    manager.FlagChanged -= FlagManagerOnFlagChanged;
                }
            });

            afterChange.Subscribe(manager =>
            {
                if (manager != null)
                {
                    manager.FlagChanged += FlagManagerOnFlagChanged;
                }
            });

            this.WhenAnyValue(x => x.SelectedWindowMode).Subscribe(mode =>
            {
                if (mode == null)
                {
                    return;
                }

                switch (mode.Value)
                {
                    case WindowModeViewModel.WindowingType.Fullscreen:
                        profile.FlagManager.SetFlag("-window", false);
                        profile.FlagManager.SetFlag("-fullscreen_window", false);
                        break;
                    case WindowModeViewModel.WindowingType.Borderless:
                        profile.FlagManager.SetFlag("-window", false);
                        profile.FlagManager.SetFlag("-fullscreen_window", true);
                        break;
                    case WindowModeViewModel.WindowingType.Windowed:
                        profile.FlagManager.SetFlag("-window", true);
                        profile.FlagManager.SetFlag("-fullscreen_window", false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private void InitializeVideoDisplaySelection(IProfile profile)
        {
            SelectedVideoDisplay =
                ResolutionCollectionView.OfType<VideoDisplayViewModel>()
                    .FirstOrDefault(display => display.Width == profile.ResolutionWidth && display.Height == profile.ResolutionHeight);

            if (SelectedVideoDisplay == null)
            {
                DetectResolution(false);
            }

            this.WhenAny(x => x.SelectedVideoDisplay, val => val.Value == null ? -1 : val.Value.Width)
                .BindTo(profile, x => x.ResolutionWidth);

            this.WhenAny(x => x.SelectedVideoDisplay, val => val.Value == null ? -1 : val.Value.Height)
                .BindTo(profile, x => x.ResolutionHeight);
        }

        private void InitializeResolutions()
        {
            var displays = SDLVideoDisplay.GetVideoDisplays().ToArray();

            IEnumerable<VideoDisplayViewModel> videoDisplayCollection;

            if (!displays.Any())
            {
                videoDisplayCollection = Enumerable.Empty<VideoDisplayViewModel>();
            }
            else
            {
                videoDisplayCollection =
                    displays.First().DisplayModes.Distinct(DisplayModeComparer).CreateDerivedCollection(mode => new VideoDisplayViewModel(mode));
            }

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
