#region Usings

using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Profiles.Keys
{
    public static class Video
    {
        public static readonly IConfigurationKey<int> ResolutionWidth = new DefaultConfigurationKey<int>("ResolutionWidth");

        public static readonly IConfigurationKey<int> ResolutionHeight = new DefaultConfigurationKey<int>("ResolutionHeight");

        public static readonly IConfigurationKey<TextureFiltering> TextureFiltering = new DefaultConfigurationKey<TextureFiltering>(
            "TextureFiltering", Interfaces.TextureFiltering.Trilinear);
    }
}
