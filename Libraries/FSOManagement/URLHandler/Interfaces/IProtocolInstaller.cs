using System.Threading.Tasks;
using FSOManagement.Annotations;

namespace FSOManagement.URLHandler.Interfaces
{
    public interface IProtocolInstaller
    {
        [NotNull]
        Task InstallHandlerAsync();
    }
}