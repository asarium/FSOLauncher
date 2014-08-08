#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Util;

#endregion

namespace FSOManagement
{
    public enum ExecutableType
    {
        FreeSpace,

        FRED
    }

    public enum ExecutableMode
    {
        Release,

        Debug
    }

    public enum ExecutableFeatureSet
    {
        SSE,

        SSE2,

        AVX
    }

    [Serializable]
    public sealed class Executable : IEquatable<Executable>
    {
        private const string GetFlagsArgument = "-get_flags";

        private const string FlagsFile = "flags.lch";

        [NonSerialized]
        private List<string> _additionTags = new List<string>();

        [NonSerialized]
        private ExecutableFeatureSet _featureSet;

        private string _fullPath;

        [NonSerialized]
        private int _major;

        [NonSerialized]
        private int _minor;

        [NonSerialized]
        private ExecutableMode _mode;

        [NonSerialized]
        private int _release;

        [NonSerialized]
        private int _revision;

        [NonSerialized]
        private ExecutableType _type;

        [UsedImplicitly]
        public Executable()
        {
        }

        public Executable(string fullPath)
        {
            OnCreated();
            FullPath = fullPath; // Setting this property will reparse the path
        }

        public ExecutableType Type
        {
            get { return _type; }
            private set { _type = value; }
        }

        public ExecutableMode Mode
        {
            get { return _mode; }
            private set { _mode = value; }
        }

        public ExecutableFeatureSet FeatureSet
        {
            get { return _featureSet; }
            private set { _featureSet = value; }
        }

        public int Major
        {
            get { return _major; }
            private set { _major = value; }
        }

        public int Minor
        {
            get { return _minor; }
            private set { _minor = value; }
        }

        public int Release
        {
            get { return _release; }
            private set { _release = value; }
        }

        public int Revision
        {
            get { return _revision; }
            private set { _revision = value; }
        }

        public string FullPath
        {
            get { return _fullPath; }
            set
            {
                _fullPath = value;
                ReparsePath();
            }
        }

        public IEnumerable<string> AdditionalTags
        {
            get { return _additionTags ?? Enumerable.Empty<string>(); }
        }

        #region IEquatable<Executable> Members

        public bool Equals(Executable other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_fullPath, other._fullPath);
        }

        #endregion

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            OnCreated();
        }

        private void OnCreated()
        {
            Major = -1;
            Minor = -1;
            Release = -1;
            Revision = -1;
            Mode = ExecutableMode.Release;
            FeatureSet = ExecutableFeatureSet.SSE2; // SSE2 is the default
            _additionTags = new List<string>();

            if (FullPath != null)
            {
                ReparsePath();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is Executable && Equals((Executable) obj);
        }

        public override int GetHashCode()
        {
            return (_fullPath != null ? _fullPath.GetHashCode() : 0);
        }

        public static bool operator ==(Executable left, Executable right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Executable left, Executable right)
        {
            return !Equals(left, right);
        }

        public string ToString(bool includeDebug)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Type == ExecutableType.FreeSpace ? "FreeSpace 2 Open" : "FRED2 Open");

            stringBuilder.AppendFormat(" {0}.{1}", Major, Minor);

            if (Release >= 0)
            {
                stringBuilder.AppendFormat(".{0}", Release);
            }

            if (Revision >= 0)
            {
                stringBuilder.AppendFormat(" r{0}", Revision);
            }

            stringBuilder.AppendFormat(" {0}", Enum.GetName(typeof(ExecutableFeatureSet), FeatureSet));

            if (includeDebug && Mode == ExecutableMode.Debug)
            {
                stringBuilder.Append(" Debug");
            }

            if (_additionTags.Any())
            {
                stringBuilder.AppendFormat(" ({0})", string.Join(", ", _additionTags));
            }

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public async Task<BuildCapabilities> GetBuildCapabilitiesAsync(CancellationToken cancellationToken)
        {
            var process = await Task.Run(() =>
            {
                var p = new Process
                {
                    StartInfo =
                        new ProcessStartInfo {FileName = FullPath, Arguments = GetFlagsArgument, WorkingDirectory = Path.GetDirectoryName(FullPath)}
                };
                p.Start();

                return p;
            }, cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            Debug.Assert(FullPath != null, "FullPath != null");

            var flagFilePath = Path.Combine(Path.GetDirectoryName(FullPath), FlagsFile);

            if (!File.Exists(flagFilePath))
            {
                return null;
            }

            BuildCapabilities caps;
            using (var fileStream = new FileStream(flagFilePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[fileStream.Length];

                await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                caps = await FlagFileReader.ReadFlagFileAsync(new MemoryStream(buffer));
            }

            File.Delete(flagFilePath);

            return caps;
        }

        #region Static functions

        private static readonly IEqualityComparer<Executable> GroupingComparerInstance = new GroupingEqualityComparer();

        public static IEqualityComparer<Executable> GroupingComparer
        {
            get { return GroupingComparerInstance; }
        }

        internal static string GlobPattern(bool fred = false)
        {
            if (fred)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    return "fred2_open*.exe";
                }

                if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    return "fred2_open*.app";
                }

                return "fred2_open*";
            }

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return "fs2_open*.exe";
            }

            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return "fs2_open*.app";
            }

            return "fs2_open*";
        }

        private void ReparsePath()
        {
            var fileName = Path.GetFileName(_fullPath);
            if (fileName == null)
            {
                throw new ArgumentException("path argument is not valid!");
            }

            _additionTags.Clear();

            var parts = fileName.Split(new[] {'_', '.', '-', ' ', '(', ')', '[', ']', '/'}, StringSplitOptions.RemoveEmptyEntries);

            var state = 0;
            foreach (var part in parts)
            {
                var lowerPart = part.ToLower();
                switch (state)
                {
                    case 0:
                        // The prefix
                        switch (lowerPart)
                        {
                            case "fs2":
                                Type = ExecutableType.FreeSpace;
                                break;
                            case "fred2":
                                Type = ExecutableType.FRED;
                                break;
                            default:
                                throw new ArgumentException(string.Format("Expected 'fs2' or 'fred2', got '{0}'!", part));
                        }
                        break;
                    case 1:
                        // The 'open' identifier
                        if (lowerPart != "open")
                        {
                            throw new ArgumentException(string.Format("Expected to see 'open', got '{0}'", part));
                        }
                        break;
                    case 2:
                        Major = int.Parse(lowerPart);
                        break;
                    case 3:
                        Minor = int.Parse(lowerPart);
                        break;
                    case 4:
                        Release = int.Parse(lowerPart);
                        break;

                    default:
                        int temp;

                        if (lowerPart[0] == 'r' && int.TryParse(lowerPart.Substring(1), out temp))
                        {
                            // revision setter
                            Revision = temp;
                        }
                        else if (lowerPart == "debug")
                        {
                            Mode = ExecutableMode.Debug;
                        }
                        else if (lowerPart.Length != 6 && !int.TryParse(lowerPart, out temp)) // This is a hack for the nightly build data tags
                        {
                            ExecutableFeatureSet featureSet;
                            if (Enum.TryParse(lowerPart, true, out featureSet))
                            {
                                FeatureSet = featureSet;
                            }
                            else
                            {
                                // Make sure to not add the extensions from Windows and Mac
                                if (lowerPart != "exe" && lowerPart != "app")
                                {
                                    _additionTags.Add(part);
                                }
                            }
                        }
                        break;
                }

                ++state;
            }

            if (state < 4)
            {
                throw new ArgumentException(string.Format("The name '{0}' is not a valid FreeSpace Open executable name!", fileName));
            }
        }

        private sealed class GroupingEqualityComparer : IEqualityComparer<Executable>
        {
            #region IEqualityComparer<Executable> Members

            public bool Equals(Executable x, Executable y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null))
                {
                    return false;
                }
                if (ReferenceEquals(y, null))
                {
                    return false;
                }
                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                if (x.AdditionalTags.Any())
                {
                    Console.WriteLine();
                }

                return x._additionTags.SequenceEqual(y._additionTags) && x._featureSet == y._featureSet && x._major == y._major &&
                       x._minor == y._minor && x._release == y._release && x._revision == y._revision && x._type == y._type;
            }

            public int GetHashCode(Executable obj)
            {
                unchecked
                {
                    var hashCode = obj._additionTags.Aggregate(397, (current, additionTag) => (current * 397) ^ additionTag.GetHashCode());

                    hashCode = (hashCode * 397) ^ (int) obj._featureSet;
                    hashCode = (hashCode * 397) ^ obj._major;
                    hashCode = (hashCode * 397) ^ obj._minor;
                    hashCode = (hashCode * 397) ^ obj._release;
                    hashCode = (hashCode * 397) ^ obj._revision;
                    hashCode = (hashCode * 397) ^ (int) obj._type;
                    return hashCode;
                }
            }

            #endregion
        }

        #endregion
    }
}
