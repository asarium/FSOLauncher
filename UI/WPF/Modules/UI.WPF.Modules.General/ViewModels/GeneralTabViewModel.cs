#region Usings

using System.ComponentModel.Composition;
using Caliburn.Micro;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 0)]
    public sealed class GeneralTabViewModel : Screen, ILauncherTab
    {
        private AudioSettingsViewModel _audioSettingsViewModel;

        private ExecutableListViewModel _executableListViewModel;

        private JoystickSettingsViewModel _joystickSettingsViewModel;

        private VideoSettingsViewModel _videoSettingsViewModel;

        public GeneralTabViewModel()
        {
            DisplayName = "General";
        }

        [Import]
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

        [Import]
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

        [Import]
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

        [Import]
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
