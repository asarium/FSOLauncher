using FSOManagement.Annotations;

namespace FSOManagement.Profiles.DataClass
{
    public struct ExecutableData
    {
        [NotNull]
        public string Path { get; set; } 
    }
}