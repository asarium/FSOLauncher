#region Usings

using System.Threading.Tasks;

#endregion

namespace UI.WPF.Modules.Installation.Interfaces
{
    public interface IInstallationState
    {
        Task<IInstallationState> WaitForNextState();
    }
}
