#region Usings

using System.Threading.Tasks;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.URLHandler.Interfaces
{
    public interface IFsoProtocolHandler
    {
        [NotNull]
        Task HandleFsoProtocolAsync([NotNull] ProtocolAction action);
    }
}
