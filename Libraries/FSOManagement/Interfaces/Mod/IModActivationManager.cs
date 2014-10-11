using System.Collections.Generic;
using System.ComponentModel;
using FSOManagement.Annotations;

namespace FSOManagement.Interfaces.Mod
{
    public interface IModActivationManager : INotifyPropertyChanged
    {
        [NotNull]
        string CommandLine { get; }

        [NotNull]
        IEnumerable<IModification> PrimaryModifications { get; }

        [NotNull]
        IEnumerable<IModification> SecondaryModifications { get; }

        [CanBeNull]
        IModification ActiveMod { get; set; }
    }
}
