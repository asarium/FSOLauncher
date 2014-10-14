#region Usings

using System;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Profiles
{
    [Serializable]
    public class DefaultConfigurationKey : IConfigurationKey
    {
        public DefaultConfigurationKey(string name)
        {
            Name = name;
        }

        #region IConfigurationKey Members

        public string Name { get; private set; }

        #endregion

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
            return Name.GetHashCode();
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
    public class DefaultConfigurationKey<TValue> : DefaultConfigurationKey, IConfigurationKey<TValue>
    {
        public DefaultConfigurationKey([NotNull] string name, TValue defaultValue = default(TValue)) : base(name)
        {
            Default = defaultValue;
        }

        #region IConfigurationKey<TValue> Members

        public TValue Default { get; private set; }


        #endregion
    }
}
