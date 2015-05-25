#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;
using Splat;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public interface IModListLoader
    {
        [NotNull]
        Task<IEnumerable<ILocalModification>> LoadModificationListAsync([NotNull] string searchFolder);
    }

    public class ModManager : IModManager, INotifyPropertyChanged
    {
        private readonly ReactiveList<IEnumerable<ILocalModification>> _modifications = new ReactiveList<IEnumerable<ILocalModification>>();

        public ModManager()
        {
            Loaders = Locator.Current.GetServices<IModListLoader>();
        }

        [NotNull]
        private IEnumerable<IModListLoader> Loaders { get; set; }

        #region IModManager Members

        public string RootFolder { set; private get; }

        public IReadOnlyReactiveList<IEnumerable<ILocalModification>> ModificationLists
        {
            get { return _modifications; }
        }

        public async Task RefreshModsAsync()
        {
            var loadTasks = Loaders.Select(loader => loader.LoadModificationListAsync(RootFolder));

            var loaded = await Task.WhenAll(loadTasks);

            _modifications.Clear();
            _modifications.AddRange(loaded);
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
