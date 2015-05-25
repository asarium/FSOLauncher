#region Usings

using System.Collections.Generic;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using ReactiveUI;

#endregion

namespace FSOManagement.Interfaces.Mod
{
    public interface IModManager
    {
        [NotNull]
        string RootFolder { set; }

        [NotNull]
        IReadOnlyReactiveList<IEnumerable<ILocalModification>> ModificationLists { get; }

        [NotNull]
        Task RefreshModsAsync();
    }
}
