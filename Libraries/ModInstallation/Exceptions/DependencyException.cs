#region Usings

using System;

#endregion

namespace ModInstallation.Exceptions
{
    public class DependencyException : Exception
    {
        public DependencyException(string message) : base(message)
        {
        }
    }
}
