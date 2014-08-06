#region Usings

using System.Collections.Generic;

#endregion

namespace UI.WPF.Launcher.Common.Classes
{
    public sealed class InstanceLaunchedMessage
    {
        public InstanceLaunchedMessage(IList<string> arguments)
        {
            Arguments = arguments;
        }

        public IList<string> Arguments { get; private set; }
    }
}
