#region Usings

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace FSOManagement.Interfaces
{
    public interface IConfigurationProvider
    {
        Task WriteConfigurationAsync(IProfile profile, CancellationToken token);
    }
}
