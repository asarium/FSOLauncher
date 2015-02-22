using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Implementations.Mods
{
    public class DefaultFileListItem : IFileListItem
    {
        #region Implementation of IFileListItem

        public IFileVerifier Verifier { get; private set; }

        public string Archive { get; private set; }

        public string Filename { get; private set; }

        public string OriginalName { get; private set; }

        #endregion

        public static DefaultFileListItem InitializeFromData(FileListItem fileListItem)
        {
            return new DefaultFileListItem
            {
                Archive = fileListItem.archive,
                Filename = fileListItem.filename,
                OriginalName = fileListItem.orig_name,
                Verifier = new Md5FileVerifier(fileListItem.md5sum)
            };
        }
    }
}