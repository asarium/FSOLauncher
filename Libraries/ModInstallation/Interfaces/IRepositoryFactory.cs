using ModInstallation.Annotations;

namespace ModInstallation.Interfaces
{
    public interface IRepositoryFactory
    {
        [NotNull]
        IModRepository ConstructRepository([NotNull] string location);
    }
}