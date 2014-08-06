using System.Collections.Generic;

namespace FSOManagement.Interfaces
{
    public interface IModDependencies
    {
        IEnumerable<string> PrimaryDependencies { get; }

        IEnumerable<string> SecondayDependencies { get; }
    }
}
