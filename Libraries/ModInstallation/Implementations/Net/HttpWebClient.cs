#region Usings

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations.Net
{
    [Export(typeof(IWebClient))]
    public class HttpWebClient : CachingWebClient
    {
        private const int BufferSize = 96 * 1024;

        public override async Task DownloadAsync(Uri uri, string destination, Action<DownloadProgress> downloadReporter, CancellationToken token)
        {
            var request = WebRequest.CreateHttp(uri);

            request.Method = "GET";
            // Use a dummy User-Agent
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:34.0) Gecko/20100101 Firefox/28.0";

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                if (response.StatusCode < HttpStatusCode.OK || response.StatusCode >= HttpStatusCode.MultipleChoices)
                {
                    throw new HttpException((int) response.StatusCode, response.StatusDescription);
                }

                var total = response.ContentLength;
                var buffer = new byte[BufferSize];
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                    {
                        throw new HttpException("No response Stream available");
                    }

                    using (var fileStream = OpenFileStream(destination))
                    {
                        var current = 0L;
                        int read;
                        while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
                        {
                            current += read;
                            token.ThrowIfCancellationRequested();

                            await fileStream.WriteAsync(buffer, 0, read, token);

                            downloadReporter(new DownloadProgress
                            {
                                Current = current,
                                Total = total
                            });
                        }
                    }
                }
            }
        }

        private static Stream OpenFileStream(string destination)
        {
            var directory = Path.GetDirectoryName(destination);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            return new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, BufferSize, FileOptions.Asynchronous);
        }

        protected override async Task<string> ActuallyGetStringAsync(Uri uri, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                var responseMessage = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
                return await responseMessage.Content.ReadAsStringAsync();
            }
        }
    }
}
