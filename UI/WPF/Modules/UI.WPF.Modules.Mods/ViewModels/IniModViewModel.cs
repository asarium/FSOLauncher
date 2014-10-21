#region Usings

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using ReactiveUI;
using Splat;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Mods.Views;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    public class IniModViewModel : ModViewModel<IniModification>
    {
        private string _displayName;

        private string _imagePath;

        private string _infoText;

        public IniModViewModel([NotNull] IObservable<string> filterObservable, [NotNull] IniModification mod) : base(mod, filterObservable)
        {
            ImagePath = GetImagePath(mod.ModRootPath, mod.Image);

            DisplayName = string.IsNullOrEmpty(mod.Name) ? mod.FolderName : mod.Name;

            InfoText = mod.Infotext ?? "<no description>";

            var moreInfoCommand = ReactiveCommand.CreateAsyncTask(async x => await OpenMoreInfoDialog());

            MoreInfoCommand = moreInfoCommand;

            InteractionService = Locator.Current.GetService<IInteractionService>();
            ProfileManager = Locator.Current.GetService<IProfileManager>();
        }

        [NotNull]
        private IInteractionService InteractionService { get; set; }

        [NotNull]
        private IProfileManager ProfileManager { get; set; }

        [NotNull]
        public ICommand MoreInfoCommand { get; private set; }

        [CanBeNull]
        public string InfoText
        {
            get { return _infoText; }
            set { RaiseAndSetIfPropertyChanged(ref _infoText, value); }
        }

        [CanBeNull]
        public string ImagePath
        {
            get { return _imagePath; }
            set { RaiseAndSetIfPropertyChanged(ref _imagePath, value); }
        }

        [CanBeNull]
        public string DisplayName
        {
            get { return _displayName; }
            set { RaiseAndSetIfPropertyChanged(ref _displayName, value); }
        }

        [NotNull]
        private async Task OpenMoreInfoDialog()
        {
            var dialog = new ModInformationDialog(this);

            await InteractionService.ShowDialog(dialog);
        }

        [CanBeNull]
        private static string GetImagePath([NotNull] string path, [CanBeNull] string image)
        {
            return string.IsNullOrEmpty(image) ? null : Path.Combine(path, image);
        }

        protected override bool IsVisible(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return true;
            }

            return DisplayName == null || DisplayName.IndexOf(filterString, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        protected override async Task<IBitmap> LoadLogoAsync()
        {
            if (!File.Exists(ImagePath))
            {
                return null;
            }

            try
            {
                return await BitmapLoader.Current.LoadFromResource(ImagePath, null, null);
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
    }
}
