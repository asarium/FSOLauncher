﻿#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using FSOManagement.Profiles.DataClass;
using Newtonsoft.Json;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Util;
using Utilities;

#endregion

namespace UI.WPF.Modules.Implementations.Implementations
{
    [Export(typeof(ISettings)), Export(typeof(ILauncherSettings))]
    public class JsonSettings : ReactiveObjectBase, ISettings
    {
        private const string SettingsFile = "settings.json";

        private readonly BehaviorSubject<ISettings> _settingsLoaded;

        private bool _checkForUpdates;

        private int _height;

        private IEnumerable<IProfile> _profiles = Enumerable.Empty<IProfile>();

        private IProfile _selectedProfile;

        private IEnumerable<TotalConversion> _totalConversions = Enumerable.Empty<TotalConversion>();

        private int _width;

        public JsonSettings()
        {
            _settingsLoaded = new BehaviorSubject<ISettings>(null);
            SetDefaultValues();
        }

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

        public IObservable<ISettings> SettingsLoaded
        {
            get { return _settingsLoaded; }
        }

        public async Task LoadAsync()
        {
            try
            {
                var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    LauncherUtils.GetApplicationName());

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
                    content = await reader.ReadToEndAsync();
                }

                var data = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<SettingsData>(content));

                SetDefaultValues();
                if (data != null)
                {
                    SetValuesFromData(data);
                }
            }
            finally
            {
                _settingsLoaded.OnNext(this);
            }
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

            var json = await jsonTask;
            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(json);
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
                SelectedProfile = SelectedProfile == null ? null : SelectedProfile.Name
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

        #region Nested type: SettingsData

        private class SettingsData
        {
            [CanBeNull]
            public IEnumerable<ProfileData> Profiles { get; set; }

            public string SelectedProfile { get; set; }

            [CanBeNull]
            public IEnumerable<TcData> TotalConversoins { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public bool CheckForUpdates { get; set; }
        }

        #endregion
    }
}