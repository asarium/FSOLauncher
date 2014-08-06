#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FSOManagement
{
    public sealed class Flag
    {
        public Flag()
        {
        }

        public Flag(string description, bool fsoOnly, string name, int offFlags, int onFlags, string type, string webUrl)
        {
            Description = description;
            FSOOnly = fsoOnly;
            Name = name;
            OffFlags = offFlags;
            OnFlags = onFlags;
            Type = type;
            WebURL = webUrl;
        }

        public string Description { get; private set; }

        public bool FSOOnly { get; private set; }

        public string Name { get; private set; }

        public int OffFlags { get; private set; }

        public int OnFlags { get; private set; }

        public string Type { get; private set; }

        public string WebURL { get; private set; }

        internal static Flag CopyFromStruct(Util.Flag flagStruct)
        {
            return new Flag
            {
                Description = flagStruct.desc,
                FSOOnly = flagStruct.fso_only,
                Name = flagStruct.name,
                OffFlags = flagStruct.off_flags,
                OnFlags = flagStruct.on_flags,
                Type = flagStruct.type,
                WebURL = flagStruct.web_url
            };
        }
    }

    public sealed class EasyFlag
    {
        public string Name { get; internal set; }

        internal static EasyFlag CopyFromStruct(Util.EasyFlag flagStruct)
        {
            return new EasyFlag {Name = flagStruct.name};
        }
    }

    [Flags]
    public enum BuildCapabilitiesFlags
    {
        None = 0,

        Openal = (1 << 0),

        NoD3D = (1 << 1),

        NewSnd = (1 << 2),
    }

    public class BuildCapabilities
    {
        private readonly List<EasyFlag> _easyFlags = new List<EasyFlag>();

        private readonly List<Flag> _flags = new List<Flag>();

        public BuildCapabilities()
        {
            BuildCaps = 0;
        }

        public IEnumerable<Flag> Flags
        {
            get { return _flags; }
        }

        public IEnumerable<EasyFlag> EasyFlags
        {
            get { return _easyFlags; }
        }

        public BuildCapabilitiesFlags BuildCaps { get; internal set; }

        internal void AddFlag(Flag flag)
        {
            _flags.Add(flag);
        }

        internal void AddEasyFlag(EasyFlag easyFlag)
        {
            _easyFlags.Add(easyFlag);
        }
    }
}
