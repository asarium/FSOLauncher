using System.Collections.Generic;
using System.Threading.Tasks;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Interfaces
{
    public interface ILocalModEnumerator
    {
        Task<IEnumerable<IInstalledModification>> FindMods(string searchPath);
    }
}