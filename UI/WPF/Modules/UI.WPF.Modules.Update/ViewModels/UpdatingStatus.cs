#region Usings

using System;
using Caliburn.Micro;
using FSOManagement.Annotations;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.ViewModels
{
    public class UpdatingStatus : PropertyChangedBase
    {
        private string _message;

        private bool _unknownProgress;

        private double _value;

        public UpdatingStatus()
        {
            Message = "Updating...";
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message)
                {
                    return;
                }
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        public bool UnknownProgress
        {
            get { return _unknownProgress; }
            set
            {
                if (value.Equals(_unknownProgress))
                {
                    return;
                }
                _unknownProgress = value;
                NotifyOfPropertyChange();
            }
        }

        public double Value
        {
            get { return _value; }
            private set
            {
                if (value.Equals(_value))
                {
                    return;
                }
                _value = value;
                NotifyOfPropertyChange();
            }
        }

        public void UpdateProgress(IUpdateProgress progress)
        {
            if (progress.State == UpdateState.Preparing || progress.State == UpdateState.Installing)
            {
                UnknownProgress = true;

                switch (progress.State)
                {
                    case UpdateState.Preparing:
                        Message = "Preparing update...";
                        break;
                    case UpdateState.Installing:
                        Message = "Installing update...";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (progress.Progress <= 0L)
                {
                    UnknownProgress = true;
                }
                else
                {
                    UnknownProgress = false;

                    Value = progress.Progress;
                    Message = string.Format("Downloading application files {0}...", GetDownloadString(progress));
                }
            }
        }

        [NotNull]
        private static string GetDownloadString([NotNull] IUpdateProgress progress)
        {
            return string.Format("({0:0}%)", progress.Progress * 100);
        }
    }
}
