#region Usings

using System;
using System.Collections.Generic;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class DefaultFileInformation : PropertyChangeBase, IFileInformation
    {
        #region IFileInformation Members

        public IEnumerable<IFileVerifier> FileVerifiers { get; private set; }

        public string Destination { get; private set; }

        public IEnumerable<Uri> DownloadUris { get; private set; }

        #endregion

        [CanBeNull]
        public static DefaultFileInformation InitializeFromData([NotNull] FileInformation fileInfo, [CanBeNull] IErrorHandler errorHandler = null)
        {
            if (fileInfo.urls == null)
            {
                if (errorHandler != null)
                    errorHandler.HandleError(fileInfo, "URL element must be present!");
                
                return null;
            }

            var newInstance = new DefaultFileInformation {Destination = fileInfo.dest};

            if (!string.IsNullOrEmpty(fileInfo.md5sum))
            {
                newInstance.FileVerifiers = new[] {new Md5FileVerifier(fileInfo.md5sum)};
            }

            var uriList = new List<Uri>();
            foreach (var url in fileInfo.urls)
            {
                Uri outUri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out outUri) || !outUri.IsFile)
                {
                    if (errorHandler != null)
                    {
                        errorHandler.HandleError(newInstance, string.Format("URL '{0}' is invalid!", url));
                    }
                }
                else
                {
                    uriList.Add(outUri);
                }
            }
            newInstance.DownloadUris = uriList;

            return newInstance;
        }
    }
}
