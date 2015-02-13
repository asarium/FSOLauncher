#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Profiles
{
    public sealed class FlagManager : IFlagManager
    {
        private readonly SortedSet<FlagInformation> _flagInformations;

        private string _commandLine;

        public FlagManager([NotNull] Profile profile)
        {
            _flagInformations = new SortedSet<FlagInformation>(profile.CommandLineOptions ?? Enumerable.Empty<FlagInformation>());

            profile.CommandLineOptions = _flagInformations;
        }

        #region IFlagManager Members

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

        public IEnumerable<FlagInformation> Flags
        {
            get { return _flagInformations; }
            set
            {
                _flagInformations.Clear();
                foreach (var flagInformation in value)
                {
                    AddFlag(flagInformation.Name, flagInformation.Value);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event FlagChangedHandler FlagChanged;

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

            foreach (var flagChangedEventArgse in eventArgs)
            {
                OnFlagChanged(flagChangedEventArgse);
            }
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

        #endregion

        private void OnFlagChanged([NotNull] FlagChangedEventArgs args)
        {
            var handler = FlagChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        [NotNull]
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
        private void OnPropertyChanged([NotNull, CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
