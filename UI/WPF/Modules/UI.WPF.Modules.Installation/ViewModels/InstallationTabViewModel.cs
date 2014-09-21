using System;
using System.ComponentModel.Composition;
using System.Threading;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Interfaces;
using UI.WPF.Launcher.Common.Interfaces;

namespace UI.WPF.Modules.Installation.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 3)]
    public sealed class InstallationTabViewModel : Screen, ILauncherTab
    {
        [NotNull]
        public IModManager ModManager { get; private set; }

        public InstallationTabViewModel()
        {
            DisplayName = "Update/Install mods";

            ModManager = new DefaultModManager();
            ModManager.AddModRepository(new WebJsonRepository("Default", "http://dev.tproxy.de/fs2/all.json"));

            ModManager.RetrieveInformationAsync(new Progress<string>(), CancellationToken.None);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }
    }
}