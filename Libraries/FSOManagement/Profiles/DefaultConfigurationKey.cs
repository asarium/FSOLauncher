#region Usings

using System;

#endregion

namespace FSOManagement.Profiles
{
    [Serializable]
    public class DefaultConfigurationKey : IConfigurationKey, IEquatable<DefaultConfigurationKey>
    {
        public DefaultConfigurationKey(string name)
        {
            Name = name;
        }

        #region IConfigurationKey Members

        public string Name { get; private set; }

        #endregion

        #region IEquatable<DefaultConfigurationKey> Members

        public bool Equals(DefaultConfigurationKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name);
        }

        #endregion

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
            return Equals((DefaultConfigurationKey) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(DefaultConfigurationKey left, DefaultConfigurationKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DefaultConfigurationKey left, DefaultConfigurationKey right)
        {
            return !Equals(left, right);
        }
    }

    [Serializable]
    public class DefaultConfigurationKey<TValue> : DefaultConfigurationKey, IConfigurationKey<TValue>, IEquatable<DefaultConfigurationKey<TValue>>
    {
        public DefaultConfigurationKey(string name, TValue defaultValue = default(TValue)) : base(name)
        {
            Default = defaultValue;
        }

        public bool Equals(DefaultConfigurationKey<TValue> other)
        {
            return other.Name == Name;
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
            return Equals((DefaultConfigurationKey<TValue>) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(DefaultConfigurationKey<TValue> left, DefaultConfigurationKey<TValue> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DefaultConfigurationKey<TValue> left, DefaultConfigurationKey<TValue> right)
        {
            return !Equals(left, right);
        }

        public TValue Default { get; private set; }
    }
}
