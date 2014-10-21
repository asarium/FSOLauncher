#region Usings

using System.Collections.Generic;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Interfaces
{
    public interface ILauncherSettings
    {
        [NotNull]
        IEnumerable<IProfile> Profiles { get; set; }

        [CanBeNull]
        IProfile SelectedProfile { get; set; }
    }
}
