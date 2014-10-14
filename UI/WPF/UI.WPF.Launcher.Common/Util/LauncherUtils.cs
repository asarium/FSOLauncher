#region Usings

using System;
using System.Reflection;
using FSOManagement.Annotations;

#endregion

namespace UI.WPF.Launcher.Common.Util
{
    public static class LauncherUtils
    {
        [NotNull]
        public static string GetApplicationName()
        {
            var assembly = Assembly.GetEntryAssembly();

            return ((AssemblyProductAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute), false)).Product;
        }
    }
}
