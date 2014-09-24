namespace ModInstallation.Implementations.DataClasses
{
    public class EnvironmentConstraint
    {
        public EnvironmentType type { get; set; }

        public ValueTypes value { get; set; }
    }

    public enum EnvironmentType
    {
        Cpu_feature,

        Os
    }

    public enum ValueTypes
    {
        None,

        SSE,

        SSE2,

        AVX,

        Windows,

        Linux,

        Macos
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
