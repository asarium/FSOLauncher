using IniParser.Model;

namespace FSOManagement.Util
{
    public static class IniDataExtensions
    {
        public static bool TryGetValue(this KeyDataCollection collection, string key, out string value, string defaultValue = null)
        {
            value = defaultValue;

            if (!collection.ContainsKey(key))
            {
                return false;
            }

            value = collection[key];
            if (value.Length > 0 && value[value.Length - 1] == ';')
            {
                value = value.Substring(0, value.Length - 1);
            }

            return true;
        }
    }
}