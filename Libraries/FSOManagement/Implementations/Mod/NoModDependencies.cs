#region Usings

using System.Collections.Generic;
using System.Linq;
using FSOManagement.Interfaces.Mod;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class NoModDependencies : IModDependencies
    {
        #region IModDependencies Members

        public IEnumerable<string> GetPrimaryDependencies(string rootPath)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetSecondayDependencies(string rootPath)
        {
            return Enumerable.Empty<string>();
        }

        #endregion
    }
}
