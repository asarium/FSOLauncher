using System;

namespace FSOManagement.Profiles
{
    [Serializable]
    internal struct FlagInformation : IComparable<FlagInformation>
    {
        private readonly string _name;

        private readonly object _value;

        public FlagInformation(string name, object value = null)
        {
            _name = name;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
        }

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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((FlagInformation) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}