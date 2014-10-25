using FSOManagement.Annotations;

namespace UI.WPF.Modules.Implementations.Implementations.DataClasses
{
    public class RepositoryData
    {
        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string Location { get; set; }
    }
}