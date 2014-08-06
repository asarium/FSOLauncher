#region Usings

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace FSOManagement.Util
{
    public static class ProcessExtensions
    {
        /// <summary>
        ///     Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token. If invoked, the task will return
        ///     immediately as cancelled.
        /// </param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        /// <remarks>Copied from http://stackoverflow.com/a/19104345/844001 </remarks>
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(false);
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(tcs.SetCanceled);
            }

            // Maybe the process has already exited
            if (process.HasExited)
            {
                tcs.TrySetResult(false);
            }

            return tcs.Task;
        }
    }
}
