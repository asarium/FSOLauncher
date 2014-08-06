#region Usings

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using ReactiveUI;
using SDLGlue;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    [Export(typeof(JoystickSettingsViewModel))]
    public class JoystickSettingsViewModel : PropertyChangedBase
    {
        private ReactiveList<JoystickViewModel> _joysticks;

        private bool _joysticksAvailable;

        private JoystickViewModel _selectedJoystick;

        [ImportingConstructor]
        public JoystickSettingsViewModel(IProfileManager profileManager)
        {
            Joysticks = new ReactiveList<JoystickViewModel>();
            Joysticks.CountChanged.Select(x => x > 0).BindTo(this, x => x.JoysticksAvailable);

            Joysticks.Add(new JoystickViewModel("<none>"));
            Joysticks.AddRange(SDLJoystick.GetJoysticks().Select(joy => new JoystickViewModel(joy)));

            SelectedJoystickPresent = GetJoystickViewModel(profileManager.CurrentProfile.SelectedJoystickGuid) != null;

            Joysticks.ItemsAdded.Subscribe(addedModel =>
            {
                if (SelectedJoystickPresent || addedModel.Joystick.GUID != profileManager.CurrentProfile.SelectedJoystickGuid)
                {
                    return;
                }

                SelectedJoystick = addedModel;
                SelectedJoystickPresent = true;
            });

            // If the profile changes, make sure that the joystick is also changed
            profileManager.WhenAny(x => x.CurrentProfile, guid => GetJoystickViewModel(guid.Value.SelectedJoystickGuid))
                .BindTo(this, x => x.SelectedJoystick);

            this.WhenAny(x => x.SelectedJoystick, val => val.Value.Joystick == null ? null : val.Value.Joystick.GUID)
                .BindTo(profileManager, x => x.CurrentProfile.SelectedJoystickGuid);

            SDLJoystick.JoystickEvent += SdlJoystickEvent;
        }

        /// <summary>
        ///     Specifies if the selected joystick was present when the view model was created. If this is <code>false</code>
        ///     adding a joystick with the saved GUID will select that joystick.
        /// </summary>
        private bool SelectedJoystickPresent { get; set; }

        public ReactiveList<JoystickViewModel> Joysticks
        {
            get { return _joysticks; }
            private set
            {
                if (Equals(value, _joysticks))
                {
                    return;
                }
                _joysticks = value;
                NotifyOfPropertyChange();
            }
        }

        public JoystickViewModel SelectedJoystick
        {
            get { return _selectedJoystick; }
            set
            {
                if (Equals(value, _selectedJoystick))
                {
                    return;
                }
                _selectedJoystick = value;
                NotifyOfPropertyChange();
            }
        }

        public bool JoysticksAvailable
        {
            get { return _joysticksAvailable; }
            set
            {
                if (value.Equals(_joysticksAvailable))
                {
                    return;
                }
                _joysticksAvailable = value;
                NotifyOfPropertyChange();
            }
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
