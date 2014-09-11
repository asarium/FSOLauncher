#region Usings

using System;
using System.Diagnostics;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using ReactiveUI;

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

            var cmd = ReactiveCommand.Create(this.WhenAny(x => x.Flag.WebURL, val => !string.IsNullOrEmpty(val.Value)));
            cmd.Subscribe(_ =>
            {
                Uri temp;
                var isValid = Uri.TryCreate(flag.WebURL, UriKind.Absolute, out temp) &&
                              (temp.Scheme == Uri.UriSchemeHttp || temp.Scheme == Uri.UriSchemeHttps);

                if (isValid)
                {
                    Process.Start(flag.WebURL);
                }
            });

            MoreInformationCommand = cmd;
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

        [NotNull]
        public ICommand MoreInformationCommand { get; private set; }

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
