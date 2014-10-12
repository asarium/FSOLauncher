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

    public interface IConfigurationKey<TValue> : IConfigurationKey
    {
        TValue Default { get; }

        [NotNull]
        object Convert(TValue value);
    }
}
