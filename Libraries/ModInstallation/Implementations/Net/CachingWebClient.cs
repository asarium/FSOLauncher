#region Usings

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations.Net
{
    public abstract class CachingWebClient : IWebClient
    {
        #region IWebClient Members

        public Task<string> GetStringAsync(Uri uri, CancellationToken token, TimeSpan? cacheDuration = null)
        {
            if (cacheDuration.HasValue)
            {
                return GetFromCache(uri, token, cacheDuration.Value);
            }

            return ActuallyGetStringAsync(uri, token);
        }

        public abstract Task DownloadAsync(Uri uri, string destination, Action<DownloadProgress> downloadReporter, CancellationToken token);

        #endregion

        [NotNull]
        protected abstract Task<string> ActuallyGetStringAsync([NotNull] Uri uri, CancellationToken token);

        [NotNull]
        private async Task<string> GetFromCache([NotNull] Uri uri, CancellationToken token, TimeSpan cacheDuration)
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
                return content;
            }

            content = await ActuallyGetStringAsync(uri, token);

            await BlobCache.LocalMachine.InsertObject(cacheKey, content, cacheDuration);

            return content;
        }
    }
}
