#region Usings

using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;

#endregion

namespace UI.WPF.Modules.Advanced.ViewModels
{
    public class FlagViewModel : PropertyChangedBase
    {
        private readonly Flag _flag;

        private readonly IFlagManager _flagManager;

        private bool _enabled;

        public FlagViewModel(Flag flag, IFlagManager flagManager)
        {
            _flag = flag;
            _flagManager = flagManager;
        }

        public string Type
        {
            get { return _flag.Type; }
        }

        public Flag Flag
        {
            get { return _flag; }
        }

        public string DisplayString
        {
            get { return string.IsNullOrEmpty(_flag.Description) ? _flag.Name : _flag.Description; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value.Equals(_enabled))
                {
                    return;
                }
                _enabled = value;
                NotifyOfPropertyChange();

                _flagManager.SetFlag(_flag.Name, value);
            }
        }

        /// <summary>
        ///     Same as Enabled but does not set the flag in the FlagManager
        /// </summary>
        /// <param name="enabled">The value to be set to Enabled</param>
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            NotifyOfPropertyChange("Enabled");
        }
    }
}
