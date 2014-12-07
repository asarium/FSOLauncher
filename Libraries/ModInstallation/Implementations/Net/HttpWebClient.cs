#region Usings

using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations.Net
{
    [Export(typeof(IWebClient))]
    public class HttpWebClient : CachingWebClient
    {
        public override async Task DownloadAsync(Uri uri, string destination, Action<DownloadProgress> downloadReporter, CancellationToken token)
        {
            using (var client = new DownloadTaskProvider())
            {
                token.Register(client.Cancel);
                await client.StartDownload(uri, destination, downloadReporter);
            }
        }

        protected override async Task<string> ActuallyGetStringAsync(Uri uri, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                var responseMessage = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
                return await responseMessage.Content.ReadAsStringAsync();
            }
        }

        #region Nested type: DownloadTaskProvider

        private class DownloadTaskProvider : IDisposable
        {
            private bool _disposed;

            private readonly WebClient _client;

            public DownloadTaskProvider()
            {
                _client = new WebClient();
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (_client != null)
                {
                    _client.Dispose();
                }

                _disposed = true;
            }

            #endregion

            public void Cancel()
            {
                if (_client != null && !_disposed)
                {
                    _client.CancelAsync();
                }
            }

            [NotNull]
            public Task StartDownload([NotNull] Uri uri, [NotNull] string destination, [NotNull] Action<DownloadProgress> downloadReporter)
            {
                _client.DownloadProgressChanged += (sender, args) => downloadReporter(new DownloadProgress
                {
                    Current = args.BytesReceived,
                    Total = args.TotalBytesToReceive
                });

                var tcs = new TaskCompletionSource<bool>();
                _client.DownloadFileCompleted += (sender, args) =>
                {
                    if (args.Cancelled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else if (args.Error != null)
                    {
                        tcs.TrySetException(args.Error);
                    }
                    else
                    {
                        tcs.TrySetResult(true);
                    }
                };

                return Task.WhenAll(Task.Run(() => _client.DownloadFileAsync(uri, destination)), tcs.Task);
            }
        }

        #endregion
    }
}
