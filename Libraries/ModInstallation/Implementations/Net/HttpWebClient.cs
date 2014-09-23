using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Interfaces.Net;

namespace ModInstallation.Implementations.Net
{
    [Export(typeof(IWebClient))]
    public class HttpWebClient : IWebClient
    {
        public async Task<IResponse> GetAsync(Uri uri, CancellationToken token, TimeSpan? cacheDuration = null)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);

                return new HttpResponse(response);
            }
        }
    }
}