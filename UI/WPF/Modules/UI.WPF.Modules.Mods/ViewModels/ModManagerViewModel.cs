using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

namespace UI.WPF.Modules.Mods.ViewModels
{
    public class ModManagerViewModel : ReactiveObjectBase
    {
        private readonly IModManager _modManager;

        private readonly IObservable<string> _filterObservable;

        private IReadOnlyReactiveList<ModListViewModel> _modLists;

        public ReactiveCommand<Unit> RefreshModsCommand { get; private set; }

        public ModManagerViewModel(IModManager modManager, IObservable<string> filterObservable, IMessageBus bus)
        {
            _modManager = modManager;
            _filterObservable = filterObservable;
            
            _modManager.WhenAnyValue(x => x.ModificationLists).Select(CreateModListsView).BindTo(this, x => x.ModLists);

            RefreshModsCommand = ReactiveCommand.CreateAsyncTask(async _ => await LoadMods());
            RefreshModsCommand.ExecuteAsync().Subscribe();

            bus.Listen<ModInstallationFinishedMessage>().InvokeCommand(RefreshModsCommand);
        }

        private async Task LoadMods()
        {
            await _modManager.RefreshModsAsync();
        }
        
        [CanBeNull]
        public IReadOnlyReactiveList<ModListViewModel> ModLists
        {
            get { return _modLists; }
            private set { RaiseAndSetIfPropertyChanged(ref _modLists, value); }
        }

        [NotNull]
        private IReadOnlyReactiveList<ModListViewModel> CreateModListsView([NotNull] IEnumerable<IEnumerable<ILocalModification>> value)
        {
            var viewModels = value.CreateDerivedCollection(mods => new ModListViewModel(mods, _filterObservable));

            // This feels like a hack but I don't know how to do it better...
            var resetSubject = new Subject<bool>();
            viewModels.CreateDerivedCollection(x => x.HasModsObservable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(resetSubject.OnNext));

            return viewModels.CreateDerivedCollection(x => x, x => x.ModViewModels.Any(), null, resetSubject);
        }
    }
}