#region Usings

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
        IReadOnlyReactiveList<IReadOnlyReactiveList<ILocalModification>> ModificationLists { get; }

        [NotNull]
        Task RefreshModsAsync();
    }
}
