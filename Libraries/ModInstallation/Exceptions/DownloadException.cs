#region Usings

using System;

#endregion

namespace ModInstallation.Exceptions
{
    public class DownloadException : Exception
    {
        public DownloadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DownloadException(string message) : base(message)
        {
        }
    }
}
