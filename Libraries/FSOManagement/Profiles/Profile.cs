#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Implementations;
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
        private static readonly IConfigurationProvider ConfigProvider = InitializeConfigProvider();

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

        public Profile(string name) : this()
        {
            _name = name;
        }

        internal Modification SelectedModification
        {
            get { return GetValue(General.SelectedModification); }
            set { SetValue(General.SelectedModification, value); }
        }

        internal SortedSet<FlagInformation> CommandLineOptions
        {
            get { return GetValue(General.CommandLineOptions); }
            set { SetValue(General.CommandLineOptions, value); }
        }

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
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

            await ConfigProvider.WriteConfigurationAsync(this, token);
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private static IConfigurationProvider InitializeConfigProvider()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new RegistryConfiurationProvider();
            }
            else
            {
                throw new NotImplementedException("Configuration for other platforms is not yet implemented...");
            }
        }

        private void OnCreated()
        {
            _modActivationManager = new ModActivationManager(this);
            _flagManager = new FlagManager(this);

            this.WhenAny(x => x.ModActivationManager.CommandLine, x => x.FlagManager.CommandLine,
                (cmd1, cmd2) => JoinCommandLine(cmd1.Value, cmd2.Value)).BindTo(this, x => x.CommandLine);

            this.WhenAny(x => x.SelectedExecutable, val => CanLaunch(val.Value)).BindTo(this, x => x.CanLaunchExecutable);
        }

        private static bool CanLaunch(Executable value)
        {
            if (value == null)
                return false;

            if (!File.Exists(value.FullPath))
                return false;

            return true;
        }

        private static string JoinCommandLine(params string[] parts)
        {
            return string.Join(" ", parts.Where(str => !string.IsNullOrEmpty(str)));
        }

        private TVal GetValue<TVal>(IConfigurationKey<TVal> key)
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

        private void SetValue<TVal>(IConfigurationKey<TVal> key, TVal value, [CallerMemberName] string propertyName = null)
        {
            if (_settingsDictionary == null)
            {
                _settingsDictionary = new Dictionary<IConfigurationKey, object>();
            }

            _settingsDictionary[key] = value;
            OnPropertyChanged(propertyName);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
