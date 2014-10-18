#region Usings

using System;
using System.Reflection;
using Utilities.Annotations;

#endregion

namespace Utilities
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
