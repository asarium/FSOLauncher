﻿#region Usings

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Profiles.DataClass;

#endregion

namespace FSOManagement.Interfaces
{
    public interface IProfile : INotifyPropertyChanged, ICloneable, IDataModel<ProfileData>
    {
        #region Properties

        [NotNull]
        string Name { get; set; }

        [NotNull]
        string CommandLine { get; }

        [NotNull]
        IObservable<bool> CanLaunchExecutable { get; }

        [NotNull]
        IFlagManager FlagManager { get; }

        [NotNull]
        IModActivationManager ModActivationManager { get; }

        #region Settings

        TextureFiltering TextureFiltering { get; set; }

        [CanBeNull]
        TotalConversion SelectedTotalConversion { get; set; }

        [CanBeNull]
        Executable SelectedExecutable { get; set; }

        [CanBeNull]
        string SelectedJoystickGuid { get; set; }

        int ResolutionWidth { get; set; }

        int ResolutionHeight { get; set; }

        [CanBeNull]
        string SelectedAudioDevice { get; set; }

        uint SampleRate { get; set; }

        bool EfxEnabled { get; set; }

        [CanBeNull]
        string SpeechVoiceName { get; set; }

        int SpeechVoiceVolume { get; set; }

        [CanBeNull]
        string ExtraCommandLine { get; set; }

        #region Voice settings

        bool UseVoiceInTechRoom { get; set; }

        bool UseVoiceInBriefing { get; set; }

        bool UseVoiceInGame { get; set; }

        bool UseVoiceInMulti { get; set; }

        #endregion

        #endregion

        #endregion

        #region Methods

        [NotNull]
        Task WriteConfigurationAsync(CancellationToken token, [NotNull] IProgress<string> progressMessages);

        [NotNull]
        Task<Process> LaunchSelectedExecutableAsync(CancellationToken token, [NotNull] IProgress<string> progressReporter);

        [NotNull]
        Task PullConfigurationAsync(CancellationToken token);

        #endregion
    }
}
