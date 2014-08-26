#region Usings

using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Util
{
    public static class ConfigurationProviderExtensions
    {
        public static bool? ReadBool(this IConfigurationProvider provider, string keyName, string section = null)
        {
            var val = provider.ReadValue<int>(keyName, section);

            if (!val.HasValue)
            {
                return null;
            }

            return val.Value != 0;
        }

        public static void WriteBool(this IConfigurationProvider provider, string keyName, string section, bool value)
        {
            provider.WriteValue(keyName, section, value ? 1 : 0);
        }
    }
}
