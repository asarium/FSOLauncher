#region Usings

using System;

#endregion

namespace ModInstallation.Exceptions
{
    public class InstallationException : Exception
    {
        public InstallationException(string message) : base(message)
        {
        }

        public InstallationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
