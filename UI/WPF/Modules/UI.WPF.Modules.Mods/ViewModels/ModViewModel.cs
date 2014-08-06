#region Usings

using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.Mods.Views;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    public class ModViewModel : PropertyChangedBase
    {
        private string _displayName;

        private string _imagePath;

        private string _infoText;

        private bool _isActiveMod;

        private bool _isPrimaryMod;

        private bool _isSecondaryMod;

        private Task _readModIniTask;

        public ModViewModel(Modification mod)
        {
            Mod = mod;

            mod.WhenAny(x => x.ModFolderPath, x => x.Image, GetImagePath).BindTo(this, x => x.ImagePath);

            mod.WhenAny(x => x.Name, x => x.FolderName, (name, folderName) => string.IsNullOrEmpty(name.Value) ? folderName.Value : name.Value)
                .BindTo(this, x => x.DisplayName);

            mod.WhenAny(x => x.Infotext, val => val.Value ?? "<no description>").BindTo(this, x => x.InfoText);

            ActivateCommand = ReactiveCommand.CreateAsyncTask(async _ => await ActivateThisMod());

            var moreInfoCommand = ReactiveCommand.CreateAsyncTask(async x => await OpenMoreInfoDialog());

            MoreInfoCommand = moreInfoCommand;
        }

        [Import]
        private IInteractionService InteractionService { get; set; }

        [Import]
        private IProfileManager ProfileManager { get; set; }

        public ICommand ActivateCommand { get; private set; }

        public ICommand MoreInfoCommand { get; private set; }

        public string InfoText
        {
            get { return _infoText; }
            private set
            {
                if (value == _infoText)
                {
                    return;
                }
                _infoText = value;
                NotifyOfPropertyChange();
            }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                if (value == _imagePath)
                {
                    return;
                }
                _imagePath = value;
                NotifyOfPropertyChange();
            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            private set
            {
                if (value == _displayName)
                {
                    return;
                }
                _displayName = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsPrimaryMod
        {
            get { return _isPrimaryMod; }
            set
            {
                if (value.Equals(_isPrimaryMod))
                {
                    return;
                }
                _isPrimaryMod = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsActiveMod
        {
            get { return _isActiveMod; }
            set
            {
                if (value.Equals(_isActiveMod))
                {
                    return;
                }
                _isActiveMod = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsSecondaryMod
        {
            get { return _isSecondaryMod; }
            set
            {
                if (value.Equals(_isSecondaryMod))
                {
                    return;
                }
                _isSecondaryMod = value;
                NotifyOfPropertyChange();
            }
        }

        public Modification Mod { get; private set; }

        private async Task ActivateThisMod()
        {
            // If the mod ini hasn't yet been read, do it
            await ReadModIniAsync(CancellationToken.None);

            ProfileManager.CurrentProfile.ModActivationManager.ActiveMod = Mod;
        }

        private async Task OpenMoreInfoDialog()
        {
            var dialog = new ModInformationDialog(this);

            await InteractionService.ShowDialog(dialog);
        }

        public async Task ReadModIniAsync(CancellationToken token)
        {
            if (_readModIniTask == null)
            {
                _readModIniTask = Mod.ReadModIniAsync(token);
            }

            await _readModIniTask;
        }

        private static string GetImagePath(IObservedChange<Modification, string> path, IObservedChange<Modification, string> image)
        {
            if (string.IsNullOrEmpty(image.Value))
            {
                return null;
            }

            return image.Value == null ? null : Path.Combine(path.Value, image.Value);
        }
    }
}
