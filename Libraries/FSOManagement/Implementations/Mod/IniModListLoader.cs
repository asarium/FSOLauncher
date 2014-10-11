using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;

namespace FSOManagement.Implementations.Mod
{
    public class IniModListLoader : IModListLoader
    {
        public async Task<IEnumerable<IModification>> LoadModificationListAsync(string searchFolder)
        {
            var modifications = GetModifications(searchFolder).ToList();

            await Task.WhenAll(modifications.OfType<Modification>().Select(mod => mod.ReadModIniAsync()));

            return modifications;
        }

        [NotNull]
        private static IEnumerable<IModification> GetModifications([NotNull] string searchFolder)
        {
            yield return new Modification(searchFolder);

            var possibleDirs = Directory.EnumerateDirectories(searchFolder);

            foreach (var possibleDir in possibleDirs.Where(possibleDir => File.Exists(Path.Combine(searchFolder, possibleDir, "mod.ini"))))
            {
                yield return new Modification(Path.Combine(searchFolder, possibleDir));
            }
        }
    }
}