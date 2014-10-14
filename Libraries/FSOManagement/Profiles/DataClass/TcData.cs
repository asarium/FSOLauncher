#region Usings

using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Profiles.DataClass
{
    public struct TcData
    {
        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string RootPath { get; set; }
    }
}
