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
        private List<string> _primaryList;

        private List<string> _secondaryList;

        public IniModDependencies([NotNull] KeyDataCollection keyDataCollection)
        {
            InitializeFromIniData(keyDataCollection);
        }

        #region IModDependencies Members

        public IEnumerable<string> GetPrimaryDependencies(string rootPath)
        {
            return _primaryList.Select(mod => Path.Combine(rootPath, mod));
        }

        public IEnumerable<string> GetSecondayDependencies(string rootPath)
        {
            return _secondaryList.Select(mod => Path.Combine(rootPath, mod));
        }

        #endregion

        private void InitializeFromIniData([NotNull] KeyDataCollection data)
        {
            string primaryList;
            string secondaryList;

            data.TryGetValue("primarylist", out primaryList, "");
            data.TryGetValue("secondarylist", out secondaryList, "");

            _primaryList = new List<string>(primaryList.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
            _secondaryList = new List<string>(secondaryList.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
