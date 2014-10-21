using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ModInstallation.Annotations;

namespace ModInstallation.Interfaces.Net
{
    public interface IResponse : IDisposable
    {
        HttpStatusCode StatusCode { get; }

        long? Length { get; }

        [NotNull]
        Task<Stream> OpenStreamAsync();
    }
}