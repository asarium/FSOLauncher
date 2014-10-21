#region Usings

using System;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Exceptions
{
    public class FileVerificationFailedException : Exception
    {
        public FileVerificationFailedException([NotNull] string filename) : base(string.Format("Failed to verify file '{0}'!", filename))
        {
        }
    }
}
