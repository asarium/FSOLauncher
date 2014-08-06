#region Usings

using System;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;

#endregion

namespace UI.WPF.Modules.General.ViewModels.Internal
{
    public class ExecutableViewModel : PropertyChangedBase
    {
        private Executable _debug;

        private string _displayString;

        private bool _hasBothVersions;

        private Executable _release;

        private bool _releaseSelected = true;

        private Executable _selectedExecutable;

        public ExecutableViewModel()
        {
            this.WhenAny(x => x.Debug, x => x.Release, (debug, release) => debug.Value != null && release.Value != null)
                .BindTo(this, x => x.HasBothVersions);

            this.WhenAny(x => x.Release, y => y.Debug, (_, __) => GetDisplayString(this)).BindTo(this, x => x.DisplayString);

            this.WhenAny(x => x.ReleaseSelected, x => x.Release, x => x.Debug, (val, b, c) => val.Value ? b.Value : c.Value)
                .BindTo(this, x => x.SelectedExecutable);
        }

        public bool HasBothVersions
        {
            get { return _hasBothVersions; }
            set
            {
                if (value.Equals(_hasBothVersions))
                {
                    return;
                }
                _hasBothVersions = value;
                NotifyOfPropertyChange();
            }
        }

        public string DisplayString
        {
            get { return _displayString; }
            private set
            {
                if (value == _displayString)
                {
                    return;
                }
                _displayString = value;
                NotifyOfPropertyChange();
            }
        }

        public Executable SelectedExecutable
        {
            get { return _selectedExecutable; }
            private set
            {
                if (Equals(value, _selectedExecutable))
                {
                    return;
                }
                _selectedExecutable = value;
                NotifyOfPropertyChange();
            }
        }

        public bool ReleaseSelected
        {
            get { return _releaseSelected; }
            set
            {
                if (value.Equals(_releaseSelected))
                {
                    return;
                }
                _releaseSelected = value;
                NotifyOfPropertyChange();
            }
        }

        public Executable Debug
        {
            get { return _debug; }
            set
            {
                if (Equals(value, _debug))
                {
                    return;
                }
                _debug = value;
                NotifyOfPropertyChange();
            }
        }

        public Executable Release
        {
            get { return _release; }
            set
            {
                if (Equals(value, _release))
                {
                    return;
                }
                _release = value;
                NotifyOfPropertyChange();
            }
        }

        private static string GetDisplayString(ExecutableViewModel value)
        {
            if (value.Release == null && value.Debug == null)
            {
                return "Unknown build";
            }

            return value.Release != null ? value.Release.ToString(false) : value.Debug.ToString(false);
        }

        public void SetExecutable(Executable exe)
        {
            switch (exe.Mode)
            {
                case ExecutableMode.Release:
                    Release = exe;
                    break;
                case ExecutableMode.Debug:
                    Debug = exe;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SelectExecutable(Executable exe)
        {
            ReleaseSelected = !Equals(Debug, exe);
        }
    }
}
