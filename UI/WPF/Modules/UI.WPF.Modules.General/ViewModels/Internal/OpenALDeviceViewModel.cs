namespace UI.WPF.Modules.General.ViewModels.Internal
{
    public class OpenALDeviceViewModel
    {
        public string Name { get; private set; }

        public OpenALDeviceViewModel(string name)
        {
            Name = name;
        }
    }
}