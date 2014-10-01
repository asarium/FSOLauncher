#region Usings

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Squirrel;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.Services
{
    [Export(typeof(IUpdateService))]
    public class SquirrelUpdateService : ReactiveObjectBase, IUpdateService
    {
        #region IUpdateService Members

        public bool IsUpdatePossible
        {
            get
            {
                using (var mgr = new UpdateManager(@"http://localhost/squirrel", "FSOLauncher", FrameworkVersion.Net45))
                {
                    Console.WriteLine(mgr.RootAppDirectory);
                }

                return false;
            }
        }

        public async Task<IUpdateStatus> CheckForUpdateAsync()
        {
            try
            {
                using (var mgr = new UpdateManager(@"http://localhost/squirrel", "FSOLauncher", FrameworkVersion.Net45))
                {
                    // Use updateManager
                    var updateInfo = await mgr.CheckForUpdate(false, Console.WriteLine);

                    Console.WriteLine(updateInfo);
                }
            }
            catch (InvalidOperationException)
            {
                return new UpdateStatus(null);
            }

            return null;
        }

        public Task DoUpdateAsync(IProgress<IUpdateProgress> progressReporter)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
