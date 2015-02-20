namespace ModInstallation.Interfaces.Mods
{
    public interface IFileListItem
    {
        IFileVerifier Verifier { get; }

        string Archive { get; }

        string Filename { get; }

        string OriginalName { get; } 
    }
}