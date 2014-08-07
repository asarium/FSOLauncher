#region Usings

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace FSOManagement.Interfaces
{
    public interface IConfigurationProvider
    {
        Task PushConfigurationAsync(IProfile profile, CancellationToken token);

        Task PullConfigurationAsync(IProfile profile, CancellationToken token);
    }
}
