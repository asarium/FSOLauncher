#region Usings

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class Md5FileVerifier : IFileVerifier
    {
        private readonly string _md5Value;

        public Md5FileVerifier([NotNull] string md5Value)
        {
            _md5Value = md5Value;
        }

        #region IFileVerifier Members

        public async Task<bool> VerifyFilePathAsync(string path, CancellationToken token, IProgress<double> progressReporter)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            const int bufferSize = 1024 * 1024;
            var buffer = new byte[bufferSize];

            var md5 = MD5.Create();

            using (var stream = File.OpenRead(path))
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, bufferSize, token)) > 0)
                {
                    if (stream.Position == stream.Length)
                    {
                        md5.TransformFinalBlock(buffer, 0, bytesRead);
                    }
                    else
                    {
                        md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    }

                    if (progressReporter != null)
                    {
                        progressReporter.Report((double) stream.Position / stream.Length);
                    }
                }
            }

            var str = new StringBuilder();

            foreach (var b in md5.Hash)
            {
                str.AppendFormat("{0:X2}", b);
            }

            return str.ToString().Equals(_md5Value, StringComparison.OrdinalIgnoreCase);
        }

        public string StringChecksum
        {
            get { return _md5Value; }
        }

        #endregion
    }
}
