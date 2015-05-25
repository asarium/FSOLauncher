#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Util;
using IniParser.Model;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class IniModDependencies : IModDependencies
    {
        #region Implementation of IModDependencies

        public int GetSupportScore(ILocalModification mod)
        {
            if (mod is IniModification)
            {
                return 1000;
            }

            return int.MinValue;
        }

        public IEnumerable<string> GetModPaths(ILocalModification mod, string rootPath)
        {
            var iniMod = mod as IniModification;

            if (iniMod == null)
                throw new NotSupportedException("Class is not supported!");


            string primaryListVal;
            string secondaryListVal;

            iniMod.Dependencies.TryGetValue("primarylist", out primaryListVal, "");
            iniMod.Dependencies.TryGetValue("secondarylist", out secondaryListVal, "");

            var primaryPaths = primaryListVal.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => Path.Combine(rootPath, p));
            var secondaryPaths = secondaryListVal.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => Path.Combine(rootPath, p));

            return primaryPaths.Concat(iniMod.ModRootPath.AsEnumerable()).Concat(secondaryPaths);
        }

        #endregion
    }
}
