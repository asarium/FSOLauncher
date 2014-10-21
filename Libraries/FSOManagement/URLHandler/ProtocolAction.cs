#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.URLHandler
{
    public enum ProtocolActionType
    {
        Focus,

        Run,

        Install,

        Settings,

        AddRepo
    }

    public class ProtocolAction
    {
        private ProtocolAction()
        {
        }

        public ProtocolActionType Action { get; private set; }

        [NotNull]
        public IList<string> Arguments { get; private set; }

        [NotNull]
        public static ProtocolAction ParseUrl([NotNull] string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("Url is not a valid FSO protocol URL!");
            }

            if (uri.Scheme != "fso")
            {
                throw new FormatException("URL scheme is not 'fso'!");
            }

            // Hehe, thanks .Net for making my life easier!
            var actionString = uri.Host.ToUpperInvariant();

            var arguments = uri.Segments.Where(seg => !seg.Equals("/")).Select(Uri.UnescapeDataString).ToList();

            ProtocolActionType type;
            switch (actionString)
            {
                case "FOCUS":
                    type = ProtocolActionType.Focus;
                    break;
                case "RUN":
                    type = ProtocolActionType.Run;
                    break;
                case "INSTALL":
                    type = ProtocolActionType.Install;
                    break;
                case "SETTINGS":
                    type = ProtocolActionType.Settings;
                    break;
                case "ADD_REPO":
                    type = ProtocolActionType.AddRepo;
                    break;
                default:
                    throw new FormatException("Unknown action '" + uri.Host + "'!");
            }

            return new ProtocolAction
            {
                Action = type,
                Arguments = arguments
            };
        }
    }
}
