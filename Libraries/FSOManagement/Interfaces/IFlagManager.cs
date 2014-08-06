#region Usings

using System.ComponentModel;

#endregion

namespace FSOManagement.Interfaces
{
    public class FlagChangedEventArgs
    {
        public FlagChangedEventArgs(string name, bool enabled, object value)
        {
            Name = name;
            Enabled = enabled;
            Value = value;
        }

        public string Name { get; private set; }

        public bool Enabled { get; private set; }

        public object Value { get; private set; }
    }

    public delegate void FlagChangedHandler(object sender, FlagChangedEventArgs args);

    public interface IFlagManager : INotifyPropertyChanged
    {
        string CommandLine { get; }

        event FlagChangedHandler FlagChanged;

        bool IsFlagSet(string name);

        void AddFlag(string name, object value = null);

        bool RemoveFlag(string name);

        void SetFlag(string name, bool present = true);
    }
}
