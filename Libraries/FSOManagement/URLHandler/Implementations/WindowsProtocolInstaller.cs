#region Usings

using System;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using FSOManagement.URLHandler.Interfaces;
using Microsoft.Win32;
using Splat;

#endregion

namespace FSOManagement.URLHandler.Implementations
{
    public class WindowsProtocolInstaller : IProtocolInstaller, IEnableLogger
    {
        #region IProtocolInstaller Members

        public Task InstallHandlerAsync()
        {
            return Task.Run((Action) CreateRegistryEntries);
        }

        #endregion

        private static bool HasAdminRights()
        {
            var identity = WindowsIdentity.GetCurrent();

            if (identity == null)
            {
                return false;
            }

            var pricipal = new WindowsPrincipal(identity);
            return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void CreateRegistryEntries()
        {
            try
            {
                using (var classesRot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default))
                {
                    using (var fsoKey = classesRot.CreateSubKey("fso"))
                    {
                        if (fsoKey == null)
                        {
                            // If we don't have admin rights, throw an exception so a process with elevated rights can be launched
                            if (!HasAdminRights())
                            {
                                throw new UnauthorizedAccessException("Failed to create key");
                            }

                            // Oh, apparently this didn't work...
                            return;
                        }

                        fsoKey.SetValue("Default", "URL:FSOLauncher protocol");
                        fsoKey.SetValue("URL Protocol", "");

                        using (var shellKey = fsoKey.CreateSubKey("shell/open/command"))
                        {
                            if (shellKey == null)
                            {
                                // If we don't have admin rights, throw an exception so a process with elevated rights can be launched
                                if (!HasAdminRights())
                                {
                                    throw new UnauthorizedAccessException("Failed to create key");
                                }

                                // Oh, apparently this didn't work...
                                return;
                            }
                            shellKey.SetValue("Default", Assembly.GetEntryAssembly().Location);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                this.Log().WarnException("Not authorized to access the registry, retrying with administrator rights.", e);
                throw new SecurityException("Unauthorized access", e);
            }
            catch (SecurityException e)
            {
                this.Log().WarnException("Not authorized to access the registry, retrying with administrator rights.", e);
                throw;
            }
        }
    }
}
