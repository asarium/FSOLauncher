#region Usings

using System.ComponentModel.Composition;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations
{
    [Export(typeof(IRepositoryFactory))]
    public class WebRepositoryFactory : IRepositoryFactory
    {
        [NotNull, Import]
        private IWebClient WebClient { get; set; }

        #region IRepositoryFactory Members

        public IModRepository ConstructRepository(string location)
        {
            return new WebJsonRepository(location) {WebClient = WebClient};
        }

        #endregion
    }
}
