#region Usings

using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace UI.WPF.Launcher.Common.Services
{
    public class UpdateVersion
    {
        public UpdateVersion(int major, int minor, int revision, int build)
        {
            this.Major = major;
            this.Minor = minor;
            this.Revision = revision;
            this.Build = build;
        }

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Revision { get; private set; }

        public int Build { get; private set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Major < 0)
            {
                return builder.ToString();
            }
            builder.Append(Major);

            if (Minor < 0)
            {
                return builder.ToString();
            }
            builder.Append('.').Append(Minor);

            if (Revision < 0)
            {
                return builder.ToString();
            }
            builder.Append('.').Append(Revision);

            if (Build != 0)
            {
                builder.Append('.').Append(Build);
            }

            return builder.ToString();
        }
    }

    public interface IUpdateStatus
    {
        UpdateVersion Version { get; }

        bool UpdateAvailable { get; }

        bool IsRequired { get; }
    }

    public enum UpdateState
    {
        Preparing,

        Downloading,

        Installing
    }

    public interface IUpdateProgress
    {
        long TotalBytes { get; }

        long CurrentBytes { get; }

        UpdateState State { get; }
    }

    public interface IUpdateService : INotifyPropertyChanged
    {
        bool IsUpdatePossible { get; }

        Task<IUpdateStatus> CheckForUpdateAsync();

        Task DoUpdateAsync(IProgress<IUpdateProgress> progressReporter);
    }
}
