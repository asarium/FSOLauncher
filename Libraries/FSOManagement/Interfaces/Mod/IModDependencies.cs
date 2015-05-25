using System;
using System.Collections.Generic;
using FSOManagement.Annotations;

namespace FSOManagement.Interfaces.Mod
{
    public interface IModDependencies
    {
        int GetSupportScore(ILocalModification mod);

        /// <summary>
        /// Gets all mods paths associated with the mod, includes the original mod at an appropriate location
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown when the given mod is not supported.</exception>
        [NotNull]
        IEnumerable<string> GetModPaths(ILocalModification mod, string rootPath);
    }
}
