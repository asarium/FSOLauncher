#region Usings

using Caliburn.Micro;
using FSOManagement.OpenAL;

#endregion

namespace UI.WPF.Modules.General.ViewModels.Internal
{
    public class OpenAlDeviceViewModel : PropertyChangedBase
    {
        public OpenAlDeviceViewModel(string name, OpenALManager.DeviceProperties properties)
        {
            Name = name;
            Properties = properties;
        }

        public string Name { get; private set; }

        private OpenALManager.DeviceProperties Properties { get; set; }

        public bool SupportsEfx
        {
            get { return Properties.SupportsEfx; }
        }

        public string Version
        {
            get { return Properties.Version; }
        }
    }
}
