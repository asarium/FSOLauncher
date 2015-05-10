#region Usings

using System.Threading.Tasks;

#endregion

namespace UI.WPF.Modules.Installation.Interfaces
{
    public enum StateResult
    {
        Continue,

        Back,

        Abort
    }

    public interface IInstallationState
    {
        Task<StateResult> GetResult();
    }
}
