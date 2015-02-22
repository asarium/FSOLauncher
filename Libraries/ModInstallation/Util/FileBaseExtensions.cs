using System.IO.Abstractions;
using System.Threading.Tasks;

namespace ModInstallation.Util
{
    public static class FileBaseExtensions
    {
        public static async Task DeleteAsync(this FileBase file, string path)
        {
            // If an exception is thrown this will rethrow it correctly
            await Task.Run(() => file.Delete(path)).ConfigureAwait(false);
        }
    }
}