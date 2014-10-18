#region Usings

using System;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    public abstract class ModViewModel : ReactiveObjectBase
    {
        private bool _visible;

        private bool _isActiveMod;

        private bool _isPrimaryMod;

        private bool _isSecondaryMod;

        private ILocalModification _mod;

        protected ModViewModel([NotNull] ILocalModification mod, [NotNull] IObservable<string> filterObservable)
        {
            _mod = mod;

            filterObservable.Subscribe(filter => Visible = IsVisible(filter));
        }

        public bool IsPrimaryMod
        {
            get { return _isPrimaryMod; }
            set { RaiseAndSetIfPropertyChanged(ref _isPrimaryMod, value); }
        }

        public bool IsActiveMod
        {
            get { return _isActiveMod; }
            set { RaiseAndSetIfPropertyChanged(ref _isActiveMod, value); }
        }

        public bool IsSecondaryMod
        {
            get { return _isSecondaryMod; }
            set { RaiseAndSetIfPropertyChanged(ref _isSecondaryMod, value); }
        }

        [NotNull]
        public virtual ILocalModification Mod
        {
            get { return _mod; }
            protected set { RaiseAndSetIfPropertyChanged(ref _mod, value); }
        }

        public bool Visible
        {
            get { return _visible; }
            private set { RaiseAndSetIfPropertyChanged(ref _visible, value); }
        }

        protected abstract bool IsVisible([CanBeNull] string filterString);
    }

    public abstract class ModViewModel<TMod> : ModViewModel where TMod : ILocalModification
    {
        protected ModViewModel([NotNull] TMod mod, [NotNull] IObservable<string> filterObservable) : base(mod, filterObservable)
        {
        }

        public TMod ModInstance
        {
            get { return (TMod) Mod; }
        }

        public override ILocalModification Mod
        {
            get { return base.Mod; }
            protected set
            {
                if (!(value is TMod))
                {
                    throw new ArgumentException("Mod instance is not of the right type!");
                }

                base.Mod = value;
            }
        }
    }
}
