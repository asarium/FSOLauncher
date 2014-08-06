#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FSOManagement.Interfaces;
using FSOManagement.Util;
using IniParser.Model;

#endregion

namespace FSOManagement.Implementations
{
    public class IniModDependencies : IModDependencies
    {
        private readonly string _name;

        private List<string> _primaryList;

        private List<string> _secondaryList;

        public IniModDependencies(string name)
        {
            _name = name;
        }

        #region IModDependencies Members

        public IEnumerable<string> PrimaryDependencies
        {
            get { return _primaryList ?? Enumerable.Empty<string>(); }
        }

        public IEnumerable<string> SecondayDependencies
        {
            get { return _secondaryList ?? Enumerable.Empty<string>(); }
        }

        #endregion

        internal void InitializeFromIniData(KeyDataCollection data)
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
