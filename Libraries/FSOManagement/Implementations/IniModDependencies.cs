#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Util;
using IniParser.Model;

#endregion

namespace FSOManagement.Implementations
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

        [NotNull]
        public IEnumerable<string> PrimaryDependencies
        {
            get { return _primaryList ?? Enumerable.Empty<string>(); }
        }

        [NotNull]
        public IEnumerable<string> SecondayDependencies
        {
            get { return _secondaryList ?? Enumerable.Empty<string>(); }
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
