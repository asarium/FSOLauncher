namespace FSOManagement.Profiles
{
    public interface IConfigurationKey
    {
        string Name { get; }
    }

    public interface IConfigurationKey<out TValue> : IConfigurationKey
    {
        TValue Default { get; }
    }
}