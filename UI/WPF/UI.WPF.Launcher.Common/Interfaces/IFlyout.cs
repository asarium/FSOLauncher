using MahApps.Metro.Controls;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IFlyout
    {
        bool IsOpen { get; }

        object Header { get; }

        Position Position { get; }
    }
}