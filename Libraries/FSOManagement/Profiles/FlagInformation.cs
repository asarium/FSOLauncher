using System;
using FSOManagement.Annotations;

namespace FSOManagement.Profiles
{
    [Serializable]
    public struct FlagInformation : IComparable<FlagInformation>
    {
        private readonly string _name;

        private readonly object _value;

        public FlagInformation([NotNull] string name, [CanBeNull] object value = null)
        {
            _name = name;
            _value = value;
        }

        [NotNull]
        public string Name
        {
            get { return _name; }
        }

        [CanBeNull]
        public object Value
        {
            get { return _value; }
        }

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