#region Usings

using System;
using System.Threading.Tasks;
using System.Xml.Schema;
using FSOManagement.Interfaces;
using Microsoft.Win32;

#endregion

namespace FSOManagement.Implementations
{
    public class RegistryConfigurationProvider : IConfigurationProvider
    {
        private const string RegistryKeyName = @"{0}_Classes\VirtualStore\Machine\Software\Wow6432Node\Volition\Freespace2";

        private static readonly string UserSid = ClsLookupAccountName.GetUserSid();

        #region IConfigurationProvider Members

        public Task ReadConfigurationAsync()
        {
            // For registry this is a no-op
            return Task.FromResult(false);
        }

        public TVal Read<TVal>(string key, string section = null) where TVal : class
        {
            using (var userKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var sectionKey = userKey.CreateSubKey(GetKeyName(section), RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (sectionKey == null)
                    {
                        throw new InvalidOperationException("Failed to open sub key!");
                    }

                    var value = sectionKey.GetValue(key);

                    if (value is TVal)
                    {
                        return (TVal) value;
                    }

                    if (value == null)
                    {
                        return null;
                    }

                    throw new InvalidOperationException("Value exists but is of the wrong type!");
                }
            }
        }

        public TVal? ReadValue<TVal>(string key, string section = null) where TVal : struct
        {
            using (var userKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var sectionKey = userKey.CreateSubKey(GetKeyName(section), RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (sectionKey == null)
                    {
                        throw new InvalidOperationException("Failed to open sub key!");
                    }

                    var value = sectionKey.GetValue(key);
                    
                    if (value is TVal)
                    {
                        return (TVal) value;
                    }

                    if (value == null)
                    {
                        return null;
                    }

                    throw new InvalidOperationException("Value exists but is of the wrong type!");
                }
            }
        }

        public void WriteValue<TVal>(string key, string section, TVal value)
        {
            using (var userKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var sectionKey = userKey.CreateSubKey(GetKeyName(section), RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (sectionKey == null)
                    {
                        throw new InvalidOperationException("Failed to open sub key!");
                    }

                    sectionKey.SetValue(key, value);
                }
            }
        }

        public bool DeleteValue(string key, string section = null)
        {
            using (var userKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var sectionKey = userKey.CreateSubKey(GetKeyName(section), RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (sectionKey == null)
                    {
                        throw new InvalidOperationException("Failed to open sub key!");
                    }

                    sectionKey.DeleteValue(key);
                }
            }

            return true;
        }

        public Task WriteConfigurationAsync()
        {
            // For registry this is a no-op
            return Task.FromResult(false);
        }

        #endregion

        private static string GetKeyName(string section)
        {
            if (section == null)
            {
                return string.Format(RegistryKeyName, UserSid);
            }
            
            return string.Format(RegistryKeyName + "\\" + string.Join("\\", section.Split('/')), UserSid);
        }
    }
}
