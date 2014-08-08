#region Usings

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace FSOManagement.Interfaces
{
    /// <summary>
    ///     Interface for a provider that can write and read profile data from the standard FreeSpace locations
    /// </summary>
    /// <remarks>
    ///     This is not an optimal solution as the keys for the data are always the same and only the underlying store changes.
    /// </remarks>
    public interface IConfigurationProvider
    {
        Task PushConfigurationAsync(IProfile profile, CancellationToken token);

        Task PullConfigurationAsync(IProfile profile, CancellationToken token);
    }
}
