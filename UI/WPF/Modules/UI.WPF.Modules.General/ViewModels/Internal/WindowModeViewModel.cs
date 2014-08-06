#region Usings

using System;

#endregion

namespace UI.WPF.Modules.General.ViewModels.Internal
{
    public class WindowModeViewModel
    {
        #region WindowingType enum

        public enum WindowingType
        {
            Fullscreen,

            Borderless,

            Windowed
        }

        #endregion

        private readonly WindowingType _value;

        public WindowModeViewModel(WindowingType type)
        {
            _value = type;
        }

        public string Name
        {
            get { return Enum.GetName(typeof(WindowingType), _value); }
        }

        public WindowingType Value
        {
            get { return _value; }
        }

        protected bool Equals(WindowModeViewModel other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((WindowModeViewModel) obj);
        }

        public override int GetHashCode()
        {
            return (int) _value;
        }
    }
}
