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
        Task<IEnumerable<IModification>> LoadModificationListAsync([NotNull] string searchFolder);
    }

    public class ModManager : IModManager, INotifyPropertyChanged
    {
        private readonly ReactiveList<IModification> _modifications = new ReactiveList<IModification>();

        public ModManager()
        {
            Loaders = Locator.Current.GetServices<IModListLoader>();
        }

        [NotNull]
        private IEnumerable<IModListLoader> Loaders { get; set; }

        #region IModManager Members

        public string RootFolder { set; private get; }

        public IReadOnlyReactiveList<IModification> Modifications
        {
            get { return _modifications; }
        }

        public async Task RefreshModsAsync()
        {
            var loadTasks = Loaders.Select(loader => loader.LoadModificationListAsync(RootFolder));

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
