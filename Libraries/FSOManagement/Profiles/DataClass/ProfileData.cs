#region Usings

using System;
using System.Collections.Generic;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Profiles.DataClass
{
    public struct ProfileData
    {
        [CanBeNull]
        public string Name { get; set; }
        
        TextureFiltering TextureFiltering { get; set; }

        TcData SelectedTotalConversion { get; set; }

        ExecutableData SelectedExecutable { get; set; }

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
    }
}
