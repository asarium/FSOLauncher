#region Usings

using System;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Profiles
{
    public class FlagInformation : IComparable<FlagInformation>
    {
        public FlagInformation([NotNull] string name, [CanBeNull] object value = null)
        {
            Name = name;
            Value = value;
        }

        [NotNull]
        public string Name { get; private set; }

        [CanBeNull]
        public object Value { get; private set; }

        #region IComparable<FlagInformation> Members

        public int CompareTo(FlagInformation other)
        {
            return String.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        #endregion

        private bool Equals(FlagInformation other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((FlagInformation) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
