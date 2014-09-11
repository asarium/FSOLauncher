#region Usings

using System.Collections.Generic;

#endregion

namespace FSOManagement.Profiles.Keys
{
    public static class General
    {
        public static readonly IConfigurationKey<Executable> SelectedExecutable = new DefaultConfigurationKey<Executable>("SelectedExecutable");

        public static readonly IConfigurationKey<TotalConversion> SelectedTotalConversion =
            new DefaultConfigurationKey<TotalConversion>("SelectedTotalConversion");

        public static readonly IConfigurationKey<string> SelectedJoystickGUID = new DefaultConfigurationKey<string>("SelectedJoystickGUID");

        public static readonly IConfigurationKey<string> VoiceName = new DefaultConfigurationKey<string>("SpeechVoiceName");

        public static readonly IConfigurationKey<int> VoiceVolume = new DefaultConfigurationKey<int>("SpeechVoiceVolume", 100);

        internal static readonly IConfigurationKey<Modification> SelectedModification =
            new DefaultConfigurationKey<Modification>("SelectedModification");

        internal static readonly IConfigurationKey<SortedSet<FlagInformation>> CommandLineOptions =
            new DefaultConfigurationKey<SortedSet<FlagInformation>>("CommandLineOptions");

        internal static readonly IConfigurationKey<string> ExtraCommandLine = new DefaultConfigurationKey<string>("ExtraCommandLine");
    }
}
