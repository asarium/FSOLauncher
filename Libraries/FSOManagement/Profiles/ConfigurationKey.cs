#region Usings

using System;
using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Profiles
{
    public interface IConfigurationKey
    {
        [NotNull]
        string Name { get; }
    }

    public interface IConfigurationKey<out TValue> : IConfigurationKey
    {
        TValue Default { get; }
    }
}
