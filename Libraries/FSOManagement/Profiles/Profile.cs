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

        private ProfileData _profileData;

        private Executable _selectedExecutable;

        private TotalConversion _selectedTotalConversion;

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
        internal SortedSet<FlagInformation> CommandLineOptions
        {
            get { return _profileData.CommandLineOptions; }
            set { _profileData.CommandLineOptions = value; }
        }

        [CanBeNull]
        public string SelectedModification
        {
            get { return _profileData.SelectedModification; }
            set { _profileData.SelectedModification = value; }
        }

        #region IProfile Members

        public bool EfxEnabled
        {
            get { return _profileData.EfxEnabled; }
            set { _profileData.EfxEnabled = value; }
        }

        public string ExtraCommandLine
        {
            get { return _profileData.ExtraCommandLine; }
            set { _profileData.ExtraCommandLine = value; }
        }

        public int ResolutionHeight
        {
            get { return _profileData.ResolutionHeight; }
            set { _profileData.ResolutionHeight = value; }
        }

        public int ResolutionWidth
        {
            get { return _profileData.ResolutionWidth; }
            set { _profileData.ResolutionWidth = value; }
        }

        public uint SampleRate
        {
            get { return _profileData.SampleRate; }
            set { _profileData.SampleRate = value; }
        }

        public string SelectedAudioDevice
        {
            get { return _profileData.SelectedAudioDevice; }
            set { _profileData.SelectedAudioDevice = value; }
        }

        public string SelectedJoystickGuid
        {
            get { return _profileData.SelectedJoystickGuid; }
            set { _profileData.SelectedJoystickGuid = value; }
        }

        public string SpeechVoiceName
        {
            get { return _profileData.SpeechVoiceName; }
            set { _profileData.SpeechVoiceName = value; }
        }

        public int SpeechVoiceVolume
        {
            get { return _profileData.SpeechVoiceVolume; }
            set { _profileData.SpeechVoiceVolume = value; }
        }

        public TextureFiltering TextureFiltering
        {
            get { return _profileData.TextureFiltering; }
            set { _profileData.TextureFiltering = value; }
        }

        public bool UseVoiceInBriefing
        {
            get { return _profileData.UseVoiceInBriefing; }
            set { _profileData.UseVoiceInBriefing = value; }
        }

        public bool UseVoiceInGame
        {
            get { return _profileData.UseVoiceInGame; }
            set { _profileData.UseVoiceInGame = value; }
        }

        public bool UseVoiceInMulti
        {
            get { return _profileData.UseVoiceInMulti; }
            set { _profileData.UseVoiceInMulti = value; }
        }

        public bool UseVoiceInTechRoom
        {
            get { return _profileData.UseVoiceInTechRoom; }
            set { _profileData.UseVoiceInTechRoom = value; }
        }

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

        public string CommandLine { get; private set; }

        public IObservable<bool> CanLaunchExecutable { get; private set; }


        public object Clone()
        {
            // Use serialization to get a truly unrealated new instance
            var newInstance = new Profile();
            newInstance.InitializeFromData(_profileData.Clone());

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

        public TotalConversion SelectedTotalConversion
        {
            get { return _selectedTotalConversion; }
            set
            {
                if (Equals(value, _selectedTotalConversion))
                {
                    return;
                }
                _selectedTotalConversion = value;
                OnPropertyChanged();
            }
        }

        public Executable SelectedExecutable
        {
            get { return _selectedExecutable; }
            set
            {
                if (Equals(value, _selectedExecutable))
                {
                    return;
                }
                _selectedExecutable = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void InitializeFromData(ProfileData data)
        {
            _profileData = data;

// ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (data.SelectedTotalConversion.RootPath != null)
            {
                SelectedTotalConversion = new TotalConversion();
                SelectedTotalConversion.InitializeFromData(data.SelectedTotalConversion);
            }
            else
            {
                SelectedTotalConversion = null;
            }

// ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (data.SelectedExecutable.Path != null)
            {
                SelectedExecutable = new Executable();
                SelectedExecutable.InitializeFromData(data.SelectedExecutable);
            }
            else
            {
                SelectedExecutable = null;
            }
        }

        public ProfileData GetData()
        {
            var ret = _profileData.Clone();

            if (SelectedExecutable != null)
            {
                ret.SelectedExecutable = SelectedExecutable.GetData();
            }

            if (SelectedTotalConversion != null)
            {
                ret.SelectedTotalConversion = SelectedTotalConversion.GetData();
            }

            return ret;
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
