using Caliburn.Micro;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface ILauncherTab : IScreen
    {
    }

    public interface ILauncherTabMetaData
    {
        int Priority { get; }
    }
}