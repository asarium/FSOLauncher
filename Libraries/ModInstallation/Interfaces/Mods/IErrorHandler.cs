using ModInstallation.Annotations;

namespace ModInstallation.Interfaces.Mods
{
    public interface IErrorHandler
    {
        bool HandleError([NotNull] object context, [NotNull] string message);
    }
}