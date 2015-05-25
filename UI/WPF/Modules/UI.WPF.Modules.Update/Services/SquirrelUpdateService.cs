#region Usings

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using Squirrel;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Services;
using Utilities;

#endregion

namespace UI.WPF.Modules.Update.Services
{
    [Export(typeof(IUpdateService))]
    public class SquirrelUpdateService : ReactiveObjectBase, IUpdateService
    {
        private const string UpdateUrl = @"http://localhost/squirrel";

        private static bool _isFirstRun;

        private UpdateInfo _updateInfo;

        #region IUpdateService Members

        public bool IsUpdatePossible
        {
            get
            {
                using (var mgr = CreateUpdateManager())
                {
                    return mgr.CurrentlyInstalledVersion() != null;
                }
            }
        }

        public bool IsFirstRun
        {
            get { return _isFirstRun; }
        }


        public async Task<IUpdateStatus> CheckForUpdateAsync()
        {
            try
            {
                using (var mgr = CreateUpdateManager())
                {
                    // Use updateManager
                    _updateInfo = await mgr.CheckForUpdate();

                    // No update found
                    if (_updateInfo == null)
                    {
                        return new UpdateStatus();
                    }

                    // Already up to date
                    if (!_updateInfo.ReleasesToApply.Any())
                    {
                        return new UpdateStatus();
                    }

                    var latest = _updateInfo.ReleasesToApply.Max(x => x.Version);

                    return new UpdateStatus(latest, true, false);
                }
            }
            catch (InvalidOperationException)
            {
                return new UpdateStatus();
            }
        }

        public async Task DoUpdateAsync(IProgress<IUpdateProgress> progressReporter)
        {
            using (var mgr = CreateUpdateManager())
            {
                progressReporter.Report(new UpdateProgress(-1.0, UpdateState.Preparing));
                if (_updateInfo == null)
                {
                    _updateInfo = await mgr.CheckForUpdate();
                }

                await
                    mgr.DownloadReleases(_updateInfo.ReleasesToApply,
                        p => progressReporter.Report(new UpdateProgress(p / 100.0, UpdateState.Downloading)));

                progressReporter.Report(new UpdateProgress(-1.0, UpdateState.Preparing));

                await mgr.ApplyReleases(_updateInfo, p => progressReporter.Report(new UpdateProgress(p / 100.0, UpdateState.Installing)));

                var releaseNotes = _updateInfo.FetchReleaseNotes().ToDictionary(p => p.Key.Version, p => p.Value);

                progressReporter.Report(UpdateProgress.Finished(releaseNotes));
            }
        }

        #endregion

        public static void UpdaterMain()
        {
            using (var mgr = CreateUpdateManager())
            {
                // ReSharper disable AccessToDisposedClosure
                SquirrelAwareApp.HandleEvents(onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                    onAppUpdate: v => mgr.CreateShortcutForThisExe(),
                    onAppUninstall: v => mgr.RemoveShortcutForThisExe(),
                    onFirstRun: () => _isFirstRun = true);
                // ReSharper restore AccessToDisposedClosure
            }
        }

        [NotNull]
        private static UpdateManager CreateUpdateManager()
        {
            return new UpdateManager(UpdateUrl, LauncherUtils.GetApplicationName());
        }
    }
}
