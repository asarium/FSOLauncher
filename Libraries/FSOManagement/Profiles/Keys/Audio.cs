namespace FSOManagement.Profiles.Keys
{
    public static class Audio
    {
        public static readonly IConfigurationKey<string> SelectedAudioDevice = new DefaultConfigurationKey<string>("SelectedAudioDevice");

        public static readonly IConfigurationKey<uint> SampleRate = new DefaultConfigurationKey<uint>("SampleRate", 44100);

        public static readonly IConfigurationKey<bool> EfxEnabled = new DefaultConfigurationKey<bool>("EfxEnabled", true);
    }
}