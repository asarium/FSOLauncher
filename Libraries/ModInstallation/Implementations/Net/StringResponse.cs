using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Net;

namespace ModInstallation.Implementations.Net
{
    public class StringResponse : IResponse
    {
        private readonly string _content;

        public StringResponse([NotNull] string content)
        {
            _content = content;
        }

        public void Dispose()
        {
        }

        public HttpStatusCode StatusCode {
            get { return HttpStatusCode.OK; }
        }

        public long? Length {
            get { return _content.Length; }
        }

        public Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(_content)));
        }
    }
}