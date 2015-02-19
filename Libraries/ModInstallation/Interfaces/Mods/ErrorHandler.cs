using ModInstallation.Annotations;

namespace ModInstallation.Interfaces.Mods
{
    public delegate bool ErrorHandler([NotNull] object context, [NotNull] string message);
}