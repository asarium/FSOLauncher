namespace UI.WPF.Launcher.Common.Services
{
    public interface ITaskbarController
    {
        bool ProgressbarVisible { set; }

        double ProgressvarValue { set; }
    }
}