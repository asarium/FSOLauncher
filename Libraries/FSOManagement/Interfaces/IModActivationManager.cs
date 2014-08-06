using System.Collections.Generic;
using System.ComponentModel;

namespace FSOManagement.Interfaces
{
    public interface IModActivationManager : INotifyPropertyChanged
    {
        string CommandLine { get; }

        IEnumerable<Modification> PrimaryModifications { get; }

        IEnumerable<Modification> SecondaryModifications { get; }

        Modification ActiveMod { get; set; }
    }
}
