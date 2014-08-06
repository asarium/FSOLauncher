using System;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Interfaces;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IExecutableLauncher
    {
        Task LaunchProfileAsync(IProfile profile, IProgress<string> progressReporter, CancellationToken token);
    }
}