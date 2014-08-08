﻿#region Usings

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
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
        private IObservableCollection<OpenAlDeviceViewModel> _devices;

        private bool _devicesAvailable;

        private OpenAlDeviceViewModel _selectedDevice;

        private bool _efxEnabled;

        private bool _efxAvailable;

        [ImportingConstructor]
        public AudioSettingsViewModel(IProfileManager profileManager)
        {
            profileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion.RootFolder).Subscribe(root =>
            {
                Devices = UpdateDevices(root);
                var defaultDevice = OpenALManager.GetDefaultDevice(OpenALManager.DeviceType.Playback, root);

                SelectedDevice = Devices.FirstOrDefault(viewModel => viewModel.Name == defaultDevice);
            });

            this.WhenAny(x => x.Devices.Count, val => val.Value > 0).BindTo(this, x => x.DevicesAvailable);

            profileManager.WhenAny(x => x.CurrentProfile.SelectedAudioDevice,
                val => val.Value == null ? SelectedDevice : Devices.FirstOrDefault(model => model.Name == val.Value))
                .BindTo(this, x => x.SelectedDevice);

            this.WhenAny(x => x.SelectedDevice, val => val.Value == null ? null : val.Value.Name)
                .BindTo(profileManager, x => x.CurrentProfile.SelectedAudioDevice);

            InitializeEfx(profileManager);

            ProfileManager = profileManager;
        }

        private void InitializeEfx(IProfileManager profileManager)
        {
            var efxEnabledObservable = profileManager.WhenAnyValue(x => x.CurrentProfile.EfxEnabled);
            var efxAvailableObservable = this.WhenAnyValue(x => x.SelectedDevice.SupportsEfx);

            efxEnabledObservable.CombineLatest(efxAvailableObservable, (enabled, available) => available && enabled).BindTo(this, x => x.EfxEnabled);

            efxAvailableObservable.BindTo(this, x => x.EfxAvailable);

            this.WhenAnyValue(x => x.EfxEnabled).BindTo(profileManager, x => x.CurrentProfile.EfxEnabled);
        }

        public IProfileManager ProfileManager { get; private set; }

        public IObservableCollection<OpenAlDeviceViewModel> Devices
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

        public bool EfxEnabled
        {
            get { return _efxEnabled; }
            set
            {
                if (value.Equals(_efxEnabled))
                {
                    return;
                }
                _efxEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool EfxAvailable
        {
            get { return _efxAvailable; }
            private set
            {
                if (value.Equals(_efxAvailable))
                {
                    return;
                }
                _efxAvailable = value;
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

        public OpenAlDeviceViewModel SelectedDevice
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

        private static IObservableCollection<OpenAlDeviceViewModel> UpdateDevices(string rootFolder)
        {
            var devices = OpenALManager.GetDevices(OpenALManager.DeviceType.Playback, rootFolder);

            return
                new BindableCollection<OpenAlDeviceViewModel>(
                    devices.Select(name => new OpenAlDeviceViewModel(name, OpenALManager.GetDeviceProperties(name, rootFolder))));
        }
    }
}
