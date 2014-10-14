#region Usings

using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Util;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 0)]
    public sealed class GeneralTabViewModel : Screen, ILauncherTab
    {
        private AudioSettingsViewModel _audioSettingsViewModel;

        private ExecutableListViewModel _executableListViewModel;

        private JoystickSettingsViewModel _joystickSettingsViewModel;

        private SpeechViewModel _speechViewModel;

        private VideoSettingsViewModel _videoSettingsViewModel;

        [ImportingConstructor]
        public GeneralTabViewModel(IProfileManager profileManager)
        {
            DisplayName = "General";

            profileManager.GetProfileObservable().Subscribe(profile =>
            {
                if (profile == null)
                {
                    ExecutableListViewModel = null;
                    VideoSettingsViewModel = null;
                    JoystickSettingsViewModel = null;
                    AudioSettingsViewModel = null;
                    SpeechViewModel = null;
                }
                else
                {
                    ExecutableListViewModel = new ExecutableListViewModel(profile);
                    VideoSettingsViewModel = new VideoSettingsViewModel(profile);
                    JoystickSettingsViewModel = new JoystickSettingsViewModel(profile);
                    AudioSettingsViewModel = new AudioSettingsViewModel(profile);
                    SpeechViewModel = new SpeechViewModel(profile);
                }
            });
        }

        public SpeechViewModel SpeechViewModel
        {
            get { return _speechViewModel; }
            private set
            {
                if (Equals(value, _speechViewModel))
                {
                    return;
                }
                _speechViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public ExecutableListViewModel ExecutableListViewModel
        {
            get { return _executableListViewModel; }
            private set
            {
                if (Equals(value, _executableListViewModel))
                {
                    return;
                }
                _executableListViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public VideoSettingsViewModel VideoSettingsViewModel
        {
            get { return _videoSettingsViewModel; }
            private set
            {
                if (Equals(value, _videoSettingsViewModel))
                {
                    return;
                }
                _videoSettingsViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public JoystickSettingsViewModel JoystickSettingsViewModel
        {
            get { return _joystickSettingsViewModel; }
            private set
            {
                if (Equals(value, _joystickSettingsViewModel))
                {
                    return;
                }
                _joystickSettingsViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public AudioSettingsViewModel AudioSettingsViewModel
        {
            get { return _audioSettingsViewModel; }
            private set
            {
                if (Equals(value, _audioSettingsViewModel))
                {
                    return;
                }
                _audioSettingsViewModel = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
