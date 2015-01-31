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

        X86_32,

        X86_64,

        Windows,

        Linux,

        Macos
    }

    public enum FeatureType
    {
        None,

        SSE,

        SSE2,

        AVX,

        X86_32,

        X86_64
    }

    public enum OsType
    {
        Windows,

        Linux,

        Macos
    }
}
