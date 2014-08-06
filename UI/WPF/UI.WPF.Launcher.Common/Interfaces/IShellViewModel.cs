using Caliburn.Micro;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IShellViewModel : IConductor
    {
        string Title { get; set; }

        ILauncherViewModel LauncherViewModel { get; }
    }
}