#region Usings

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;

#endregion

namespace FSOManagement.Implementations.Mod
{
    [Export(typeof(IModListLoader))]
    public class IniModListLoader : IModListLoader
    {
        #region IModListLoader Members

        public async Task<IReadOnlyReactiveList<ILocalModification>> LoadModificationListAsync(string searchFolder)
        {
            var modifications = new ReactiveList<ILocalModification>(GetModifications(searchFolder));

            await
                Task.WhenAll(modifications.OfType<IniModification>().Select(async mod => await mod.ReadModIniAsync().ConfigureAwait(false)))
                    .ConfigureAwait(false);

            return modifications;
        }

        #endregion

        [NotNull]
        private static IEnumerable<ILocalModification> GetModifications([NotNull] string searchFolder)
        {
            yield return new IniModification(searchFolder);

            var possibleDirs = Directory.EnumerateDirectories(searchFolder);

            foreach (var possibleDir in possibleDirs.Where(possibleDir => File.Exists(Path.Combine(searchFolder, possibleDir, "mod.ini"))))
            {
                yield return new IniModification(Path.Combine(searchFolder, possibleDir));
            }
        }
    }
}
