using System.ComponentModel.Composition;
using System.Windows.Shell;
using UI.WPF.Launcher.Common.Services;

namespace UI.WPF.Launcher.Implementations
{
    [Export(typeof(ITaskbarController))]
    public class MetroTaskbarController : MetroWindowController, ITaskbarController
    {
        public bool ProgressbarVisible
        {
            set
            {
                if (Window == null)
                    return;

                Window.TaskbarItemInfo.ProgressState = value ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.None;
            }
        }

        public double ProgressvarValue
        {
            set
            {
                if (Window == null)
                    return;

                Window.TaskbarItemInfo.ProgressValue = value;
            }
        }
    }
}