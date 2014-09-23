#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations.Net
{
    [Export(typeof(IWebClient))]
    public class HttpWebClient : IWebClient
    {
        #region IWebClient Members

        public async Task<IResponse> GetAsync(Uri uri, CancellationToken token, TimeSpan? cacheDuration = null)
        {
            if (cacheDuration.HasValue)
            {
                return await GetFromCache(uri, token, cacheDuration.Value);
            }

            return new HttpResponse(await GetResponse(uri, token));
        }

        #endregion

        [NotNull]
        private static async Task<IResponse> GetFromCache([NotNull] Uri uri, CancellationToken token, TimeSpan cacheDuration)
        {
            var cacheKey = uri.ToString();
            string content = null;
            try
            {
                content = await BlobCache.LocalMachine.GetObject<string>(cacheKey);
            }
            catch (KeyNotFoundException)
            {
                // Key is not in the cache
            }

            if (content != null)
            {
                return new StringResponse(content);
            }

            var response = await GetResponse(uri, token);

            content = await response.Content.ReadAsStringAsync();

            await BlobCache.LocalMachine.InsertObject(cacheKey, content, cacheDuration);

            return new HttpResponse(response);
        }

        [NotNull]
        private static async Task<HttpResponseMessage> GetResponse([NotNull] Uri uri, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                return await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
            }
        }
    }
}
