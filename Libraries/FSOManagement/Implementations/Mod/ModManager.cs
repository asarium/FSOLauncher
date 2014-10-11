#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class ModManager : IModManager, INotifyPropertyChanged
    {
        private readonly ReactiveList<IModification> _modifications = new ReactiveList<IModification>();

        private readonly string _rootFolder;

        public ModManager([NotNull] string rootFolder)
        {
            _rootFolder = rootFolder;
        }

        #region IModManager Members

        public IReadOnlyReactiveList<IModification> Modifications
        {
            get { return _modifications; }
        }

        public async Task RefreshModsAsync(CancellationToken token)
        {
            var modifications = GetOldModifications().ToList();

            await Task.WhenAll(modifications.OfType<Modification>().Select(mod => mod.ReadModIniAsync(token)));

            _modifications.Clear();
            _modifications.AddRange(modifications);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotNull]
        private IEnumerable<IModification> GetOldModifications()
        {
            yield return new Modification(_rootFolder);

            var possibleDirs = Directory.EnumerateDirectories(_rootFolder);

            foreach (var possibleDir in possibleDirs.Where(possibleDir => File.Exists(Path.Combine(_rootFolder, possibleDir, "mod.ini"))))
            {
                yield return new Modification(Path.Combine(_rootFolder, possibleDir));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull, CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
