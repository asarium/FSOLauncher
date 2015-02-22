using System.IO.Abstractions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ModInstallation.Util
{
    public static class JSONUtils
    {
        public static async Task<T> ParseJSONFile<T>(this IFileSystem fileSystem, string filePath)
        {
            string content;
            using (var reader = fileSystem.File.OpenText(filePath))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            return await Task.Run(() => JsonConvert.DeserializeObject<T>(content)).ConfigureAwait(false);
        }
    }
}