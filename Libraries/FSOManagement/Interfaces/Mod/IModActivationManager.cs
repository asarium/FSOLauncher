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
        IEnumerable<ILocalModification> PrimaryModifications { get; }

        [NotNull]
        IEnumerable<ILocalModification> SecondaryModifications { get; }

        [CanBeNull]
        ILocalModification ActiveMod { get; set; }
    }
}
