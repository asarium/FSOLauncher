using System.Threading.Tasks;
using Caliburn.Micro;
using FSOManagement.Annotations;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IShellViewModel : IConductor
    {
        [NotNull]
        string Title { get; set; }

        [NotNull]
        ILauncherViewModel LauncherViewModel { get; }

        bool OverlayVisible { get; set; }
    }
}