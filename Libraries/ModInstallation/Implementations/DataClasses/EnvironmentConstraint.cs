namespace ModInstallation.Implementations.DataClasses
{
    public class EnvironmentConstraint
    {
        public EnvironmentType type { get; set; }

        public FeatureType feature { get; set; }

        public OsType os { get; set; }
    }

    public enum EnvironmentType
    {
        Cpu_feature,

        Os
    }

    public enum FeatureType
    {
        None,

        SSE,

        SSE2,

        AVX
    }

    public enum OsType
    {
        Windows,

        Linux,

        Macos
    }
}