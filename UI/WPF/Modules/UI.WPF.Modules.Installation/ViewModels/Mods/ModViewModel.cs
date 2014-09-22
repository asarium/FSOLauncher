#region Usings

using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class ModViewModel : ReactiveObjectBase
    {
        private IModification _mod;

        private bool _hasDescription;

        public ModViewModel([NotNull] IModification mod)
        {
            _mod = mod;

            mod.WhenAny(x => x.Description, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasDescription);
        }

        public bool HasDescription
        {
            get { return _hasDescription; }
            private set { RaiseAndSetIfPropertyChanged(ref _hasDescription, value); }
        }

        [NotNull]
        public IModification Mod
        {
            get { return _mod; }
            private set { RaiseAndSetIfPropertyChanged(ref _mod, value); }
        }
    }
}
