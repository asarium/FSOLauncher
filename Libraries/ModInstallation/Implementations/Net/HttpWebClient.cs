#region Usings

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ModInstallation.Exceptions;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations.Net
{
    [Export(typeof(IWebClient))]
    public class HttpWebClient : CachingWebClient
    {
        private const int BufferSize = 96 * 1024;

        static HttpWebClient()
        {
            // Initialize the SSL validation callback
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
        }

        private static bool ServerCertificateValidationCallback(object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain == null)
                {
                    return true;
                }

                foreach (var status in chain.ChainStatus)
                {
                    if ((certificate.Subject == certificate.Issuer) && (status.Status == X509ChainStatusFlags.UntrustedRoot))
                    {
                        // Self-signed certificates with an untrusted root are valid.
                        continue;
                    }
                    
                    if (status.Status != X509ChainStatusFlags.NoError)
                    {
                        // If there are any other errors in the certificate chain, the certificate is invalid,
                        // so the method returns false.
                        return false;
                    }
                }
                // When processing reaches this line, the only errors in the certificate chain are
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange Server installations, so return true.
                return true;
            }
            
            // In all other cases, return false.
            return false;
        }

        public override async Task DownloadAsync(Uri uri, string destination, Action<DownloadProgress> downloadReporter, CancellationToken token)
        {
            var request = WebRequest.CreateHttp(uri);

            request.Method = "GET";
            // Use a dummy User-Agent
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:34.0) Gecko/20100101 Firefox/28.0";
            try
            {
                using (var response = (HttpWebResponse) await request.GetResponseAsync().ConfigureAwait(false))
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
                            while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) != 0)
                            {
                                current += read;
                                token.ThrowIfCancellationRequested();

                                await fileStream.WriteAsync(buffer, 0, read, token).ConfigureAwait(false);

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
            catch (IOException e)
            {
                throw new DownloadException("Exception while downloading!", e);
            }
            catch (WebException e)
            {
                throw new DownloadException("Web-Exception while downloading", e);
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
