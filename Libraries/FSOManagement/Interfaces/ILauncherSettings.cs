#region Usings

using System.Collections.Generic;
#endregion

namespace FSOManagement.Interfaces
{
    public interface ILauncherSettings
    {
        IEnumerable<IProfile> Profiles { get; set; }

        IProfile SelectedProfile { get; set; }
    }
}
