using FSOManagement.Annotations;

namespace FSOManagement.Interfaces.Mod
{
    public interface IModification
    {
        [NotNull]
        string ModRootPath { get; }

        [NotNull]
        IModDependencies Dependencies { get; }
    }
}