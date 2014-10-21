#region Usings

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;
using Splat;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    public abstract class ModViewModel : ReactiveObjectBase
    {
        private bool _isActiveMod;

        private bool _isPrimaryMod;

        private bool _isSecondaryMod;

        private bool _logoLoaded;

        private ImageSource _logoSource;

        private ILocalModification _mod;

        private bool _visible;

        private bool _loadingLogo;

        protected ModViewModel([NotNull] ILocalModification mod, [NotNull] IObservable<string> filterObservable)
        {
            ProfileManager = Locator.Current.GetService<IProfileManager>();

            _mod = mod;

            filterObservable.Subscribe(filter => Visible = IsVisible(filter));

            var activateCommand = ReactiveCommand.Create();
            activateCommand.Subscribe(_ => ActivateThisMod());
            ActivateCommand = activateCommand;
        }

        [NotNull]
        public ICommand ActivateCommand { get; private set; }

        [NotNull]
        private IProfileManager ProfileManager { get; set; }

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

        [CanBeNull]
        public ImageSource LogoSource
        {
            get
            {
                if (_logoLoaded)
                {
                    return _logoSource;
                }

                _logoLoaded = true;
                LoadingLogo = true;
                LoadLogoAsync().ContinueWith(task =>
                {
                    LoadingLogo = false;
                    if (task.Result != null)
                    {
                        LogoSource = task.Result.ToNative();
                    }
                });

                return _logoSource;
            }
            private set { RaiseAndSetIfPropertyChanged(ref _logoSource, value); }
        }

        public bool LoadingLogo
        {
            get { return _loadingLogo; }
            private set { RaiseAndSetIfPropertyChanged(ref _loadingLogo, value); }
        }

        private void ActivateThisMod()
        {
            if (ProfileManager.CurrentProfile == null)
            {
                return;
            }

            ProfileManager.CurrentProfile.ModActivationManager.ActiveMod = Mod;
        }

        protected abstract bool IsVisible([CanBeNull] string filterString);

        [NotNull]
        protected abstract Task<IBitmap> LoadLogoAsync();
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
