using System.ComponentModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using FSOManagement.Annotations;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IShellViewModel : INotifyPropertyChanged
    {
        [NotNull]
        string Title { get; set; }

        [CanBeNull]
        ILauncherViewModel LauncherViewModel { get; }

        bool OverlayVisible { get; set; }
    }
}