#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Tests.TestClasses
{
    internal class TestResponse : IResponse
    {
        [CanBeNull]
        public string Content { private get; set; }

        #region IResponse Members

        public void Dispose()
        {
        }

        public HttpStatusCode StatusCode { get; set; }

        public long? Length
        {
            get
            {
                if (Content != null)
                {
                    return Content.Length;
                }

                return null;
            }
        }

        public Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(Content ?? "")));
        }

        #endregion
    }

    internal class TimeoutResponse : TestResponse
    {
    }

    public class TestWebClient : IWebClient
    {
        private readonly Queue<string> _responseQueue = new Queue<string>();

        #region IWebClient Members

        public Task<string> GetStringAsync(Uri uri, CancellationToken token, TimeSpan? cacheDuration)
        {
            var result = _responseQueue.Dequeue();

            if (result != null)
            {
                return Task.FromResult(result);
            }

            var tcs = new TaskCompletionSource<string>();
            tcs.SetException(new HttpRequestException("Timeout!"));

            return tcs.Task;
        }

        public Task DownloadAsync(Uri uri, string destination, Action<DownloadProgress> downloadReporter, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        [NotNull]
        public TestWebClient RespondWith([CanBeNull] string content = null)
        {
            _responseQueue.Enqueue(content);

            return this;
        }

        [NotNull]
        public TestWebClient SimulateTimeout()
        {
            _responseQueue.Enqueue(null);

            return this;
        }
    }
}
