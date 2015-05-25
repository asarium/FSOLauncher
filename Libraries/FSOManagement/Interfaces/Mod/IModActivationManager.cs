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
        IEnumerable<ILocalModification> ActivatedMods { get; }

        [CanBeNull]
        ILocalModification ActiveMod { get; set; }
    }
}
