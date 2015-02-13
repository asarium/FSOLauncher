#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using FSOManagement.Annotations;
using FSOManagement.Profiles;

#endregion

namespace FSOManagement.Interfaces
{
    public class FlagChangedEventArgs
    {
        public FlagChangedEventArgs([NotNull] string name, bool enabled, [CanBeNull] object value)
        {
            Name = name;
            Enabled = enabled;
            Value = value;
        }

        [NotNull]
        public string Name { get; private set; }

        public bool Enabled { get; private set; }

        [CanBeNull]
        public object Value { get; private set; }
    }

    public delegate void FlagChangedHandler(object sender, FlagChangedEventArgs args);

    public interface IFlagManager : INotifyPropertyChanged
    {
        [NotNull]
        string CommandLine { get; }

        [NotNull]
        IEnumerable<FlagInformation> Flags { get; set; }

        event FlagChangedHandler FlagChanged;

        bool IsFlagSet([NotNull] string name);

        void AddFlag([NotNull] string name, [CanBeNull] object value = null);

        bool RemoveFlag([NotNull] string name);

        void SetFlag([NotNull] string name, bool present = true);
    }
}
