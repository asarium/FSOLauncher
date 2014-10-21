using FSOManagement.Annotations;

namespace FSOManagement.Interfaces.Mod
{
    public interface ILocalModification
    {
        [NotNull]
        string ModRootPath { get; }

        [NotNull]
        IModDependencies Dependencies { get; }
    }
}