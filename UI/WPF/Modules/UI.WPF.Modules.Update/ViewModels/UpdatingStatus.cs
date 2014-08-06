#region Usings

using System;
using Caliburn.Micro;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.ViewModels
{
    public class UpdatingStatus : PropertyChangedBase
    {
        private double _current;

        private double _maximum;

        private string _message;

        private bool _unknownProgress;

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

        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if (value.Equals(_maximum))
                {
                    return;
                }
                _maximum = value;
                NotifyOfPropertyChange();
            }
        }

        public double Current
        {
            get { return _current; }
            set
            {
                if (value.Equals(_current))
                {
                    return;
                }
                _current = value;
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
                if (progress.TotalBytes <= 0L)
                {
                    UnknownProgress = true;
                }
                else
                {
                    UnknownProgress = false;

                    Maximum = progress.TotalBytes;
                    Current = progress.CurrentBytes;
                    Message = string.Format("Downloading application files {0}...", GetDownloadString(progress));
                }
            }
        }

        private static string GetDownloadString(IUpdateProgress progress)
        {
            return string.Format("({0} of {1})", progress.CurrentBytes.HumanReadableByteCount(false),
                progress.TotalBytes.HumanReadableByteCount(false));
        }
    }

    internal static class Utilities
    {
        public static String HumanReadableByteCount(this long bytes, bool si)
        {
            var unit = si ? 1000 : 1024;
            if (bytes < unit)
            {
                return bytes + " B";
            }

            var exp = (int) (Math.Log(bytes) / Math.Log(unit));
            var pre = (si ? "kMGTPE" : "KMGTPE")[exp - 1] + (si ? "" : "i");

            return String.Format("{0:0.0} {1}B", bytes / Math.Pow(unit, exp), pre);
        }
    }
}
