#region Usings

using System.Collections.Generic;
using System.Linq;
using FSOManagement.Interfaces.Mod;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class NoModDependencies : IModDependencies
    {
        public NoModDependencies()
        {
        }

        #region IModDependencies Members

        public IEnumerable<string> PrimaryDependencies
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<string> SecondayDependencies
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        #endregion
    }
}
