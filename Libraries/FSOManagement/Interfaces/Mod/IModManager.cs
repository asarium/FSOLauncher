using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using ReactiveUI;

namespace FSOManagement.Interfaces.Mod
{
    public interface IModManager
    {
        [NotNull]
        string RootFolder { set; }

        [NotNull]
        IReadOnlyReactiveList<IModification> Modifications { get; }

        [NotNull]
        Task RefreshModsAsync();
    }
}