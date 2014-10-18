using System.Collections.Generic;
using FSOManagement.Annotations;

namespace FSOManagement.Interfaces.Mod
{
    public interface IModDependencies
    {
        [NotNull]
        IEnumerable<string> GetPrimaryDependencies([NotNull] string rootPath);

        [NotNull]
        IEnumerable<string> GetSecondayDependencies([NotNull] string rootPath);
    }
}
