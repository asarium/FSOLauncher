#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Implementations.DataClasses
{
    public enum ActionType
    {
        Delete,

        Move,

        Copy,

        Mkdir
    }

    public class ActionData
    {
        public ActionType type { get; set; }

        [CanBeNull]
        public IEnumerable<string> paths { get; set; }

        public bool glob { get; set; }

        [CanBeNull]
        public string dest { get; set; }
    }
}
