#region Usings

using System.Collections.Generic;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Profiles.DataClass
{
    public class ProfileData
    {
        public ProfileData()
        {
            Name = "";
            SettingsDictionary = new Dictionary<string, object>();
        }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public Dictionary<string, object> SettingsDictionary { get; private set; }
    }
}
