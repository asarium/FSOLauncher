#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Profiles
{
    public class FlagManager : IFlagManager
    {
        private readonly SortedSet<FlagInformation> _flagInformations;

        private readonly Profile _profile;

        private string _commandLine;

        public FlagManager(Profile profile)
        {
            _profile = profile;
            _flagInformations = profile.CommandLineOptions ?? new SortedSet<FlagInformation>();

            profile.CommandLineOptions = _flagInformations;
        }

        public string CommandLine
        {
            get { return _commandLine; }
            private set
            {
                if (value == _commandLine)
                {
                    return;
                }
                _commandLine = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [field: NonSerialized]
        public event FlagChangedHandler FlagChanged;

        protected virtual void OnFlagChanged(FlagChangedEventArgs args)
        {
            var handler = FlagChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public bool IsFlagSet(string name)
        {
            return _flagInformations.Contains(new FlagInformation(name));
        }

        public void AddFlag(string name, object value = null)
        {
            _flagInformations.Add(new FlagInformation(name, value));

            OnFlagChanged(new FlagChangedEventArgs(name, true, value));
            UpdateCommandLine();
        }

        public bool RemoveFlag(string name)
        {
            var eventArgs =
                _flagInformations.Where(info => info.Name == name).Select(info => new FlagChangedEventArgs(name, false, info.Value)).ToList();

            var result = _flagInformations.RemoveWhere(info => info.Name == name) > 0;

            eventArgs.Apply(OnFlagChanged);
            UpdateCommandLine();

            return result;
        }

        public void SetFlag(string name, bool present = true)
        {
            if (present)
            {
                AddFlag(name);
            }
            else
            {
                RemoveFlag(name);
            }
        }

        private IEnumerable<string> GetCommandlineFragments()
        {
            foreach (var flagInformation in _flagInformations)
            {
                if (flagInformation.Value != null)
                {
                    var valueString = flagInformation.Value.ToString();

                    if (valueString.Contains(" "))
                    {
                        // If there are spaces in the value, put " around it
                        valueString = string.Format("\"{0}\"", valueString);
                    }

                    yield return string.Format("{0} {1}", flagInformation.Name, valueString);
                }
                else
                {
                    yield return flagInformation.Name;
                }
            }
        }

        private void UpdateCommandLine()
        {
            CommandLine = string.Join(" ", GetCommandlineFragments());
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
