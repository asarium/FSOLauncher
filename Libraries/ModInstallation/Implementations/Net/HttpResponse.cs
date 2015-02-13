#region Usings

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations.Net
{
    public class HttpResponse : IResponse
    {
        private readonly HttpResponseMessage _message;

        public HttpResponse([NotNull] HttpResponseMessage message)
        {
            _message = message;
        }

        #region IResponse Members

        public void Dispose()
        {
            if (_message != null)
            {
                _message.Dispose();
            }
        }

        public HttpStatusCode StatusCode
        {
            get { return _message.StatusCode; }
        }

        public long? Length {
            get { return _message.Content.Headers.ContentLength; }
        }

        public Task<Stream> OpenStreamAsync()
        {
            return _message.Content.ReadAsStreamAsync();
        }

        #endregion
    }
}
