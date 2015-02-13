#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using FSOManagement.Annotations;

#endregion

namespace UI.WPF.Launcher.Common.Services
{
    public interface IUpdateStatus
    {
        [NotNull]
        Version Version { get; }

        bool UpdateAvailable { get; }

        bool IsRequired { get; }
    }

    public enum UpdateState
    {
        Preparing,

        Downloading,

        Installing,

        Finished
    }

    public interface IUpdateProgress
    {
        double Progress { get; }

        UpdateState State { get; }

        [CanBeNull]
        IDictionary<Version, string> ReleaseNotes { get; }
    }

    public interface IUpdateService : INotifyPropertyChanged
    {
        bool IsUpdatePossible { get; }

        bool IsFirstRun { get; }

        [NotNull]
        Task<IUpdateStatus> CheckForUpdateAsync();

        [NotNull]
        Task DoUpdateAsync([NotNull] IProgress<IUpdateProgress> progressReporter);
    }
}
