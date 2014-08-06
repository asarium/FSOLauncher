#region Usings

using Caliburn.Micro;
using SDLGlue;

#endregion

namespace UI.WPF.Modules.General.ViewModels.Internal
{
    public class JoystickViewModel : PropertyChangedBase
    {
        public JoystickViewModel(SDLJoystick joystick)
        {
            Joystick = joystick;
            DisplayString = Joystick.Name;
        }

        public JoystickViewModel(string displayString)
        {
            DisplayString = displayString;
        }

        public SDLJoystick Joystick { get; private set; }

        public string DisplayString { get; private set; }
    }
}
