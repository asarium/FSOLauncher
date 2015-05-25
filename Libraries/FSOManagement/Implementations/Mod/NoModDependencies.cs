#region Usings

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FSOManagement.Interfaces.Mod;

#endregion

namespace FSOManagement.Implementations.Mod
{
    [Export(typeof(IModDependencies))]
    public class NoModDependencies : IModDependencies
    {
        #region Implementation of IModDependencies

        public int GetSupportScore(ILocalModification mod)
        {
            return -1000;
        }

        public IEnumerable<string> GetModPaths(ILocalModification mod, string rootPath)
        {
            yield return mod.ModRootPath;
        }

        #endregion
    }
}
