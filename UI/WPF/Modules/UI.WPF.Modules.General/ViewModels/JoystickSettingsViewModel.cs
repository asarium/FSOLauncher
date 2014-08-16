#region Usings

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using FSOManagement.Interfaces;
using ReactiveUI;
using SDLGlue;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Util;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    public class JoystickSettingsViewModel : ReactiveObjectBase
    {
        private ReactiveList<JoystickViewModel> _joysticks;

        private bool _joysticksAvailable;

        private JoystickViewModel _selectedJoystick;

        public JoystickSettingsViewModel(IProfile profile)
        {
            InitializeJoystickList(profile);

            InitializeCurrentJoystickBinding(profile);
        }

        /// <summary>
        ///     Specifies if the selected joystick was present when the view model was created. If this is <code>false</code>
        ///     adding a joystick with the saved GUID will select that joystick.
        /// </summary>
        private bool SelectedJoystickPresent { get; set; }

        public ReactiveList<JoystickViewModel> Joysticks
        {
            get { return _joysticks; }
            private set { RaiseAndSetIfPropertyChanged(ref _joysticks, value); }
        }

        public JoystickViewModel SelectedJoystick
        {
            get { return _selectedJoystick; }
            set { RaiseAndSetIfPropertyChanged(ref _selectedJoystick, value); }
        }

        public bool JoysticksAvailable
        {
            get { return _joysticksAvailable; }
            set { RaiseAndSetIfPropertyChanged(ref _joysticksAvailable, value); }
        }

        private void InitializeCurrentJoystickBinding(IProfile profile)
        {
            profile.WhenAny(x => x.SelectedJoystickGuid, val => GetJoystickViewModel(val.Value)).BindTo(this, x => x.SelectedJoystick);

            this.WhenAny(x => x.SelectedJoystick, val => val.Value.Joystick == null ? null : val.Value.Joystick.GUID)
                .BindTo(profile, x => x.SelectedJoystickGuid);
        }

        private void InitializeJoystickList(IProfile profile)
        {
            Joysticks = new ReactiveList<JoystickViewModel>();
            Joysticks.CountChanged.Select(x => x > 0).BindTo(this, x => x.JoysticksAvailable);

            Joysticks.Add(new JoystickViewModel("<none>"));
            Joysticks.AddRange(SDLJoystick.GetJoysticks().Select(joy => new JoystickViewModel(joy)));

            SelectedJoystickPresent = GetJoystickViewModel(profile.SelectedJoystickGuid) != null;

            Joysticks.ItemsAdded.Subscribe(addedModel =>
            {
                if (SelectedJoystickPresent || addedModel.Joystick.GUID != profile.SelectedJoystickGuid)
                {
                    return;
                }

                SelectedJoystick = addedModel;
                SelectedJoystickPresent = true;
            });

            SDLJoystick.JoystickEvent += SdlJoystickEvent;
        }

        private JoystickViewModel GetJoystickViewModel(string guid)
        {
            if (guid == null)
            {
                return Joysticks.FirstOrDefault(model => model.Joystick == null);
            }

            return Joysticks.Where(model => model.Joystick != null).FirstOrDefault(model => model.Joystick.GUID == guid);
        }

        private void SdlJoystickEvent(SDLJoystickEventArgs args)
        {
            switch (args.Type)
            {
                case SDLJoystickEventType.Added:
                    Joysticks.Add(new JoystickViewModel(args.Joystick));
                    break;
                case SDLJoystickEventType.Removed:
                    foreach (var viewModel in Joysticks.Where(model => model.Joystick == args.Joystick).ToArray())
                    {
                        Joysticks.Remove(viewModel);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
