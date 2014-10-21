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
        private readonly Queue<TestResponse> _responseQueue = new Queue<TestResponse>();

        #region IWebClient Members

        public Task<IResponse> GetAsync(Uri uri, CancellationToken token, TimeSpan? cacheDuration = null)
        {
            var result = _responseQueue.Dequeue();

            if (!(result is TimeoutResponse))
            {
                return Task.FromResult<IResponse>(result);
            }

            var tcs = new TaskCompletionSource<IResponse>();
            tcs.SetException(new HttpRequestException("Timeout!"));

            return tcs.Task;
        }

        #endregion

        [NotNull]
        public TestWebClient RespondWith(int code, [CanBeNull] string content = null)
        {
            return RespondWith((HttpStatusCode) code, content);
        }

        [NotNull]
        public TestWebClient RespondWith(HttpStatusCode statusCode, [CanBeNull] string content = null)
        {
            _responseQueue.Enqueue(new TestResponse {Content = content, StatusCode = statusCode});

            return this;
        }

        [NotNull]
        public TestWebClient SimulateTimeout()
        {
            _responseQueue.Enqueue(new TimeoutResponse());

            return this;
        }
    }
}
