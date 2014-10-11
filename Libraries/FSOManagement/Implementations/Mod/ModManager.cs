#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public interface IModListLoader
    {
        [NotNull]
        Task<IEnumerable<IModification>> LoadModificationListAsync([NotNull] string searchFolder);
    }

    public class ModManager : IModManager, INotifyPropertyChanged
    {
        private static readonly IEnumerable<IModListLoader> Loaders = new[] {new IniModListLoader()};

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

        public async Task RefreshModsAsync()
        {
            var loadTasks = Loaders.Select(loader => loader.LoadModificationListAsync(_rootFolder));

            var loaded = await Task.WhenAll(loadTasks);

            _modifications.Clear();
            _modifications.AddRange(loaded.SelectMany(x => x));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

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
