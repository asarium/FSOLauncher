#region Usings

using System.Collections.Generic;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Profiles.DataClass
{
    public struct ProfileData
    {
        [CanBeNull]
        public IEnumerable<FlagInformation> CommandLineOptions { get; set; }

        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string SelectedModification { get; set; }

        public TextureFiltering TextureFiltering { get; set; }

        public TcData SelectedTotalConversion { get; set; }

        public ExecutableData SelectedExecutable { get; set; }

        [CanBeNull]
        public string SelectedJoystickGuid { get; set; }

        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        [CanBeNull]
        public string SelectedAudioDevice { get; set; }

        public uint SampleRate { get; set; }

        public bool EfxEnabled { get; set; }

        [CanBeNull]
        public string SpeechVoiceName { get; set; }

        public int SpeechVoiceVolume { get; set; }

        [CanBeNull]
        public string ExtraCommandLine { get; set; }

        #region Voice settings

        public bool UseVoiceInTechRoom { get; set; }

        public bool UseVoiceInBriefing { get; set; }

        public bool UseVoiceInGame { get; set; }

        public bool UseVoiceInMulti { get; set; }

        #endregion

        public ProfileData Clone()
        {
            var ret = this;

            if (CommandLineOptions != null)
            {
                ret.CommandLineOptions = new List<FlagInformation>(CommandLineOptions);
            }

            return ret;
        }
    }
}
