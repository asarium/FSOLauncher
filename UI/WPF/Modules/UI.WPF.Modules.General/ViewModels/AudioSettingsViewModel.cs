#region Usings

using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using FSOManagement.OpenAL;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    [Export(typeof(AudioSettingsViewModel))]
    public class AudioSettingsViewModel : PropertyChangedBase
    {
        private IObservableCollection<OpenALDeviceViewModel> _devices;

        private bool _devicesAvailable;

        private OpenALDeviceViewModel _selectedDevice;

        [ImportingConstructor]
        public AudioSettingsViewModel(IProfileManager profileManager)
        {
            profileManager.WhenAny(x => x.CurrentProfile.SelectedTotalConversion.RootFolder, val => UpdateDevices(val.Value))
                .BindTo(this, x => x.Devices);

            this.WhenAny(x => x.Devices.Count, val => val.Value > 0).BindTo(this, x => x.DevicesAvailable);

            profileManager.WhenAny(x => x.CurrentProfile.SelectedAudioDevice,
                val => val.Value == null ? Devices.FirstOrDefault() : Devices.FirstOrDefault(model => model.Name == val.Value))
                .BindTo(this, x => x.SelectedDevice);

            this.WhenAny(x => x.SelectedDevice, val => val.Value == null ? null : val.Value.Name)
                .BindTo(profileManager, x => x.CurrentProfile.SelectedAudioDevice);
        }

        public IObservableCollection<OpenALDeviceViewModel> Devices
        {
            get { return _devices; }
            private set
            {
                if (Equals(_devices, value))
                {
                    return;
                }

                _devices = value;
                NotifyOfPropertyChange();
            }
        }

        public bool DevicesAvailable
        {
            get { return _devicesAvailable; }
            private set
            {
                if (value.Equals(_devicesAvailable))
                {
                    return;
                }
                _devicesAvailable = value;
                NotifyOfPropertyChange();
            }
        }

        public OpenALDeviceViewModel SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (Equals(value, _selectedDevice))
                {
                    return;
                }
                _selectedDevice = value;
                NotifyOfPropertyChange();
            }
        }

        private static IObservableCollection<OpenALDeviceViewModel> UpdateDevices(string rootFolder)
        {
            var devices = OpenALManager.GetDevices(OpenALManager.DeviceType.Playback, rootFolder);

            return new BindableCollection<OpenALDeviceViewModel>(devices.Select(name => new OpenALDeviceViewModel(name)));
        }
    }
}
