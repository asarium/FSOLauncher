using System.Collections.Generic;
using System.Linq;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using FSOManagement.Interfaces.Mod;

namespace FSOManagement.Util
{
    public static class IModManagerExtensions
    {
        [NotNull]
        public static IEnumerable<ILocalModification> GetModifications([NotNull] this IModManager This)
        {
            return This.ModificationLists.Aggregate(Enumerable.Empty<ILocalModification>(), (prev, val) => prev.Concat(val));
        }
    }
}