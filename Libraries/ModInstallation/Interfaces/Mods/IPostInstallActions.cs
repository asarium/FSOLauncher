using System.Threading.Tasks;
using FSOManagement.Annotations;

namespace ModInstallation.Interfaces.Mods
{
    public interface IPostInstallActions
    {
        [NotNull]
        Task ExecuteActionsAsync([NotNull] string installFolder);
    }
}