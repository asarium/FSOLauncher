#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Interfaces.Mods
{
    public interface IFileVerifier
    {
        [NotNull]
        Task<bool> VerifyFilePathAsync([NotNull] string path, CancellationToken token, [CanBeNull] IProgress<double> progress = null);
    }
}
