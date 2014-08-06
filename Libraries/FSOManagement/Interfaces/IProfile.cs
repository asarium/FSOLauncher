#region Usings

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace FSOManagement.Interfaces
{
    public interface IProfile : INotifyPropertyChanged
    {
        #region Properties

        string Name { get; }

        string CommandLine { get; }

        bool CanLaunchExecutable { get; }

        #region Settings

        TextureFiltering TextureFiltering { get; set; }

        TotalConversion SelectedTotalConversion { get; set; }

        Executable SelectedExecutable { get; set; }

        string SelectedJoystickGuid { get; set; }

        int ResolutionWidth { get; set; }

        int ResolutionHeight { get; set; }

        IFlagManager FlagManager { get; }

        IModActivationManager ModActivationManager { get; }

        #endregion

        #endregion

        #region Methods

        Task WriteConfigurationAsync(CancellationToken token, IProgress<string> progressMessages);

        Task<Process> LaunchSelectedExecutableAsync(CancellationToken token, IProgress<string> progressReporter);

        #endregion
    }
}
