namespace FSOManagement.Profiles.Keys
{
    public static class Speech
    {
        public static readonly IConfigurationKey<bool> UseInTechRoom = new DefaultConfigurationKey<bool>("UseInTechRoom");

        public static readonly IConfigurationKey<bool> UseInBriefing = new DefaultConfigurationKey<bool>("UseInBriefing");

        public static readonly IConfigurationKey<bool> UseInGame = new DefaultConfigurationKey<bool>("UseInGame");

        public static readonly IConfigurationKey<bool> UseInMulti = new DefaultConfigurationKey<bool>("UseInMulti");
    }
}
