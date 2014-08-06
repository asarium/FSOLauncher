#region Usings

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Interfaces;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Implementations.Implementations
{
    [Export(typeof(IExecutableLauncher))]
    public class ExecutableLauncher : IExecutableLauncher
    {
        #region IExecutableLauncher Members

        public async Task LaunchProfileAsync(IProfile profile, IProgress<string> progressReporter, CancellationToken token)
        {
            await profile.WriteConfigurationAsync(token, progressReporter);

            if (token.IsCancellationRequested)
                return;


        }

        #endregion
    }
}
