#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using FSOManagement.Profiles.DataClass;
using ModInstallation.Interfaces;
using Newtonsoft.Json;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Implementations.Implementations.DataClasses;
using Utilities;

#endregion

namespace UI.WPF.Modules.Implementations.Implementations
{
    [Export(typeof(ISettings)), Export(typeof(ILauncherSettings))]
    public class JsonSettings : ReactiveObjectBase, ISettings
    {
        private const string SettingsFile = "settings.json";

        private bool _checkForUpdates;

        private int _height;

        private IEnumerable<IModRepositoryViewModel> _modRepositories;

        private IEnumerable<IProfile> _profiles = Enumerable.Empty<IProfile>();

        private IProfile _selectedProfile;

        private IEnumerable<TotalConversion> _totalConversions = Enumerable.Empty<TotalConversion>();

        private int _width;

        public JsonSettings()
        {
            SetDefaultValues();
        }

        [Import, NotNull]
        public IRepositoryFactory RepositoryFactory { get; private set; }

        #region ISettings Members

        public IEnumerable<IProfile> Profiles
        {
            get { return _profiles; }
            set { RaiseAndSetIfPropertyChanged(ref _profiles, value); }
        }

        public IProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set { RaiseAndSetIfPropertyChanged(ref _selectedProfile, value); }
        }

        public IEnumerable<TotalConversion> TotalConversions
        {
            get { return _totalConversions; }
            set { RaiseAndSetIfPropertyChanged(ref _totalConversions, value); }
        }

        public IEnumerable<IModRepositoryViewModel> ModRepositories
        {
            get { return _modRepositories; }
            set { RaiseAndSetIfPropertyChanged(ref _modRepositories, value); }
        }

        public int Width
        {
            get { return _width; }
            set { RaiseAndSetIfPropertyChanged(ref _width, value); }
        }

        public int Height
        {
            get { return _height; }
            set { RaiseAndSetIfPropertyChanged(ref _height, value); }
        }

        public bool CheckForUpdates
        {
            get { return _checkForUpdates; }
            set { RaiseAndSetIfPropertyChanged(ref _checkForUpdates, value); }
        }

        public async Task LoadAsync()
        {
            var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LauncherUtils.GetApplicationName());

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, SettingsFile);

            if (!File.Exists(filePath))
            {
                // Nothing to load, set default values
                SetDefaultValues();
                return;
            }

            string content;
            using (var reader = new StreamReader(filePath))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            var data = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<SettingsData>(content)).ConfigureAwait(false);

            // Not get back onto the main thread to set the variables
            await Observable.Start(() =>
            {
                SetDefaultValues();
                if (data != null)
                {
                    SetValuesFromData(data);
                }
            },
                RxApp.MainThreadScheduler);
        }

        public async Task SaveAsync()
        {
            var data = GetData();

            var jsonTask = Task.Factory.StartNew(() => JsonConvert.SerializeObject(data, Formatting.Indented),
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LauncherUtils.GetApplicationName());

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, SettingsFile);

            var json = await jsonTask.ConfigureAwait(false);
            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(json).ConfigureAwait(false);
            }
        }

        #endregion

        private void SetDefaultValues()
        {
            Width = 620;
            Height = 865;
            CheckForUpdates = true;

            TotalConversions = Enumerable.Empty<TotalConversion>();
            Profiles = Enumerable.Empty<IProfile>();
            SelectedProfile = null;
            ModRepositories = null;
        }

        private void SetValuesFromData([NotNull] SettingsData data)
        {
            Width = data.Width;
            Height = data.Height;
            CheckForUpdates = data.CheckForUpdates;

            if (data.TotalConversoins != null)
            {
                TotalConversions = data.TotalConversoins.Select(GetTcInstance).ToList();
            }

            if (data.Profiles != null)
            {
                Profiles = data.Profiles.Select(GetProfileInstance).ToList();
            }

            if (data.SelectedProfile != null)
            {
                SelectedProfile = Profiles.FirstOrDefault(p => p.Name == data.SelectedProfile);
            }

            if (data.Repositories != null)
            {
                ModRepositories = data.Repositories.Where(r => r.Location != null && r.Name != null).Select(repo => new ModRepositoryViewModel
                {
                    Name = repo.Name,
                    Repository = RepositoryFactory.ConstructRepository(repo.Location)
                }).ToList();
            }
        }

        [NotNull]
        private SettingsData GetData()
        {
            return new SettingsData
            {
                Height = Height,
                Width = Width,
                CheckForUpdates = CheckForUpdates,
                TotalConversoins = TotalConversions.Select(tc => tc.GetData()),
                Profiles = Profiles.Select(p => p.GetData()),
                SelectedProfile = SelectedProfile == null ? null : SelectedProfile.Name,
                Repositories = ModRepositories.Select(vm => new RepositoryData
                {
                    Name = vm.Name,
                    Location = vm.Repository.Name
                })
            };
        }

        [NotNull]
        private static TotalConversion GetTcInstance(TcData arg)
        {
            var tc = new TotalConversion();
            tc.InitializeFromData(arg);

            return tc;
        }

        [NotNull]
        private static IProfile GetProfileInstance(ProfileData arg)
        {
            var profile = new Profile();
            profile.InitializeFromData(arg);

            return profile;
        }
    }
}
