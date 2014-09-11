#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles.Keys;
using FSOManagement.Util;
using ReactiveUI;

#endregion

namespace FSOManagement.Profiles
{
    [Serializable]
    public class Profile : IProfile, IDeserializationCallback
    {
        [NonSerialized]
        private bool _canLaunchExecutable;

        [NonSerialized]
        private string _commandLine;

        [NonSerialized]
        private FlagManager _flagManager;

        [NonSerialized]
        private ModActivationManager _modActivationManager;

        private string _name;

        private Dictionary<IConfigurationKey, object> _settingsDictionary;

        private Profile()
        {
            OnCreated();
        }

        public Profile([NotNull] string name) : this()
        {
            _name = name;
        }

        [CanBeNull]
        internal Modification SelectedModification
        {
            get { return GetValue(General.SelectedModification); }
            set { SetValue(General.SelectedModification, value); }
        }

        [NotNull]
        internal SortedSet<FlagInformation> CommandLineOptions
        {
            get { return GetValue(General.CommandLineOptions); }
            set { SetValue(General.CommandLineOptions, value); }
        }

        #region IDeserializationCallback Members

        public void OnDeserialization([CanBeNull] object sender)
        {
            if (_settingsDictionary != null)
            {
                // Something is wrong with the dictionary implementation
                // See http://stackoverflow.com/a/457204/844001
                ((IDeserializationCallback) _settingsDictionary).OnDeserialization(sender);
            }

            OnCreated();

            ModActivationManager.ActiveMod = SelectedModification;
        }

        #endregion

        #region IProfile Members

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                {
                    return;
                }
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool CanLaunchExecutable
        {
            get { return _canLaunchExecutable; }
            private set
            {
                if (value.Equals(_canLaunchExecutable))
                {
                    return;
                }
                _canLaunchExecutable = value;
                OnPropertyChanged();
            }
        }

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

        public object Clone()
        {
            // Use serialization to get a truly unreated new instance
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                stream.Seek(0, SeekOrigin.Begin);

                return formatter.Deserialize(stream);
            }
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
                    StartInfo =
                        new ProcessStartInfo
                        {
                            FileName = SelectedExecutable.FullPath,
                            WorkingDirectory = Path.GetDirectoryName(SelectedExecutable.FullPath)
                        }
                };
                process.Start();

                return process;
            }, token);

            return launchedProcess;
        }

        public Task PullConfigurationAsync(CancellationToken token)
        {
            return ConfigurationManager.ReadConfigurationAsync(this);
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

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

        private void OnCreated()
        {
            _modActivationManager = new ModActivationManager(this);
            _flagManager = new FlagManager(this);

            this.WhenAny(x => x.ModActivationManager.CommandLine, x => x.FlagManager.CommandLine,
                (cmd1, cmd2) => JoinCommandLine(cmd1.Value, cmd2.Value)).BindTo(this, x => x.CommandLine);

            this.WhenAny(x => x.SelectedExecutable, val => CanLaunch(val.Value)).BindTo(this, x => x.CanLaunchExecutable);
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
            if (_settingsDictionary == null)
            {
                return key.Default;
            }

            object value;
            if (!_settingsDictionary.TryGetValue(key, out value))
            {
                return key.Default;
            }

            if (value is TVal)
            {
                return (TVal) (value);
            }

            return default(TVal);
        }

        private void SetValue<TVal>([NotNull] IConfigurationKey<TVal> key, TVal value, [NotNull, CallerMemberName] string propertyName = null)
        {
            if (_settingsDictionary == null)
            {
                _settingsDictionary = new Dictionary<IConfigurationKey, object>();
            }

            _settingsDictionary[key] = value;
            OnPropertyChanged(propertyName);
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
