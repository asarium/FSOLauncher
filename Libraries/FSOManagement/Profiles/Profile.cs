#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using FSOManagement.Interfaces;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Profiles.DataClass;
using FSOManagement.Profiles.Keys;
using FSOManagement.Util;
using ReactiveUI;
using Splat;

#endregion

namespace FSOManagement.Profiles
{
    public class Profile : IProfile, IEnableLogger
    {
        private readonly FlagManager _flagManager;

        private readonly ModActivationManager _modActivationManager;

        private string _commandLine;

        private ProfileData _profileData;

        public Profile()
        {
            _modActivationManager = new ModActivationManager(this);
            _flagManager = new FlagManager(this);

            this.WhenAny(x => x.ModActivationManager.CommandLine,
                x => x.FlagManager.CommandLine,
                x => x.ExtraCommandLine,
                (cmd1, cmd2, cmd3) => JoinCommandLine(cmd1.Value, cmd2.Value, cmd3.Value)).BindTo(this, x => x.CommandLine);

            CanLaunchExecutable = this.WhenAny(x => x.SelectedExecutable, val => CanLaunch(val.Value));
        }

        [CanBeNull]
        internal string SelectedModification
        {
            get { return GetValue(General.SelectedModificationFolder); }
            set { SetValue(General.SelectedModificationFolder, value); }
        }

        [NotNull]
        internal SortedSet<FlagInformation> CommandLineOptions
        {
            get { return GetValue(General.CommandLineOptions); }
            set { SetValue(General.CommandLineOptions, value); }
        }

        #region IProfile Members

        public string Name
        {
            get { return _profileData.Name; }
            set
            {
                if (value == _profileData.Name)
                {
                    return;
                }
                _profileData.Name = value;
                OnPropertyChanged();
            }
        }

        public IObservable<bool> CanLaunchExecutable { get; private set; }

        public TextureFiltering TextureFiltering
        {
            get { return GetValue(Video.TextureFiltering); }
            set { SetValue(Video.TextureFiltering, value); }
        }

        public TotalConversion SelectedTotalConversion
        {
            get { return GetValue(General.SelectedTotalConversion); }
            set { SetValue(General.SelectedTotalConversion, value); }
        }

        public Executable SelectedExecutable
        {
            get { return GetValue(General.SelectedExecutable); }
            set { SetValue(General.SelectedExecutable, value); }
        }

        public string SelectedJoystickGuid
        {
            get { return GetValue(General.SelectedJoystickGUID); }
            set { SetValue(General.SelectedJoystickGUID, value); }
        }

        public int ResolutionWidth
        {
            get { return GetValue(Video.ResolutionWidth); }
            set { SetValue(Video.ResolutionWidth, value); }
        }

        public int ResolutionHeight
        {
            get { return GetValue(Video.ResolutionHeight); }
            set { SetValue(Video.ResolutionHeight, value); }
        }

        public string SelectedAudioDevice
        {
            get { return GetValue(Audio.SelectedAudioDevice); }
            set { SetValue(Audio.SelectedAudioDevice, value); }
        }

        public uint SampleRate
        {
            get { return GetValue(Audio.SampleRate); }
            set { SetValue(Audio.SampleRate, value); }
        }

        public bool EfxEnabled
        {
            get { return GetValue(Audio.EfxEnabled); }
            set { SetValue(Audio.EfxEnabled, value); }
        }

        public string SpeechVoiceName
        {
            get { return GetValue(General.VoiceName); }
            set { SetValue(General.VoiceName, value); }
        }

        public int SpeechVoiceVolume
        {
            get { return GetValue(General.VoiceVolume); }
            set { SetValue(General.VoiceVolume, value); }
        }

        public string ExtraCommandLine
        {
            get { return GetValue(General.ExtraCommandLine); }
            set { SetValue(General.ExtraCommandLine, value); }
        }

        public object Clone()
        {
            // Use serialization to get a truly unrealated new instance
            var newInstance = new Profile();
            newInstance.InitializeFromData(_profileData);

            return newInstance;
        }

        public IFlagManager FlagManager
        {
            get { return _flagManager; }
        }

        public IModActivationManager ModActivationManager
        {
            get { return _modActivationManager; }
        }

        public string CommandLine
        {
            get { return _commandLine; }
            private set
            {
                if (value == _commandLine)
                {
                    return;
                }
                _commandLine = value;
                OnPropertyChanged();
            }
        }

        public async Task WriteConfigurationAsync(CancellationToken token, IProgress<string> progressMessages)
        {
            if (SelectedTotalConversion == null)
            {
                progressMessages.Report("No total conversion selected!");
                return;
            }

            var rootPath = SelectedTotalConversion.RootFolder;

            var cmdlineConfig = Path.Combine(rootPath, "data", FsoConstants.CmdlineConfigFile);

            progressMessages.Report("Writing commandline file...");

            using (var stream = new FileStream(cmdlineConfig, FileMode.Create, FileAccess.Write))
            {
                var buffer = Encoding.UTF8.GetBytes(CommandLine.ToCharArray());
                await stream.WriteAsync(buffer, 0, buffer.Length, token);
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            progressMessages.Report("Writing configurations...");

            await ConfigurationManager.WriteConfigurationAsync(this);
        }

        public async Task<Process> LaunchSelectedExecutableAsync(CancellationToken token, IProgress<string> progressReporter)
        {
            if (SelectedExecutable == null || !File.Exists(SelectedExecutable.FullPath))
            {
                progressReporter.Report("Failed to launch because of invalid selected executable.");
                return null;
            }

            await WriteConfigurationAsync(token, progressReporter);

            if (token.IsCancellationRequested)
            {
                return null;
            }

            if (SelectedExecutable.FullPath == null)
            {
                return null;
            }

            // Do this in the background as it may block for a short while
            var launchedProcess = await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SelectedExecutable.FullPath,
                        WorkingDirectory = Path.GetDirectoryName(SelectedExecutable.FullPath)
                    }
                };
                process.Start();

                return process;
            },
                token);

            return launchedProcess;
        }

        public Task PullConfigurationAsync(CancellationToken token)
        {
            return ConfigurationManager.ReadConfigurationAsync(this);
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void InitializeFromData(ProfileData data)
        {
            _profileData = data;
        }

        public ProfileData GetData()
        {
            return _profileData;
        }

        #endregion

        #region Voice settings

        public bool UseVoiceInTechRoom
        {
            get { return GetValue(Keys.Speech.UseInTechRoom); }
            set { SetValue(Keys.Speech.UseInTechRoom, value); }
        }

        public bool UseVoiceInBriefing
        {
            get { return GetValue(Keys.Speech.UseInBriefing); }
            set { SetValue(Keys.Speech.UseInBriefing, value); }
        }

        public bool UseVoiceInGame
        {
            get { return GetValue(Keys.Speech.UseInGame); }
            set { SetValue(Keys.Speech.UseInGame, value); }
        }

        public bool UseVoiceInMulti
        {
            get { return GetValue(Keys.Speech.UseInMulti); }
            set { SetValue(Keys.Speech.UseInMulti, value); }
        }

        #endregion

        protected bool Equals(Profile other)
        {
            return string.Equals(_profileData.Name, other._profileData.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Profile) obj);
        }

        public override int GetHashCode()
        {
            return _profileData.Name == null ? 31 : _profileData.Name.GetHashCode();
        }

        private static bool CanLaunch([CanBeNull] Executable value)
        {
            if (value == null)
            {
                return false;
            }

            if (!File.Exists(value.FullPath))
            {
                return false;
            }

            return true;
        }

        [NotNull]
        private static string JoinCommandLine([NotNull] params string[] parts)
        {
            return string.Join(" ", parts.Where(str => !string.IsNullOrEmpty(str)));
        }

        private TVal GetValue<TVal>([NotNull] IConfigurationKey<TVal> key)
        {
            if (_profileData.Settings == null)
            {
                return key.Default;
            }

            object value;
            if (!_profileData.Settings.TryGetValue(key.Name, out value))
            {
                return key.Default;
            }

            if (value == null)
            {
                return key.Default;
            }

            // If it's a data model get the data
            if (!IsDataModelOf(typeof(TVal), value.GetType()))
            {
                this.Log().Warn("Settings key {0} should be data type of {1} but is type {2}!", key.Name, typeof(TVal), value.GetType());
                return key.Default;
            }

            var modelInstance = Activator.CreateInstance<TVal>();

            var initializeMethod = typeof(TVal).GetMethod("InitializeFromData");
            initializeMethod.Invoke(modelInstance, new[] {value});

            return modelInstance;
        }

        private void SetValue<TVal>([NotNull] IConfigurationKey key, TVal value, [NotNull, CallerMemberName] string propertyName = null)
        {
            object val = value;
            if (value is IDataModel)
            {
                dynamic dynValue = value;
                val = dynValue.GetData();
            }

            if (_profileData.Settings == null)
            {
                _profileData.Settings = new Dictionary<string, object>();
            }

            _profileData.Settings[key.Name] = val;
            OnPropertyChanged(propertyName);
        }

        private static bool IsDataModelOf([NotNull] Type type, [NotNull] Type dataType)
        {
            var interfaces = type.GetInterfaces();
            return
                interfaces.Any(
                    x =>
                        x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDataModel<>) && x.GetGenericArguments().Any(arg => arg == dataType));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CanBeNull, CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
