using System;

namespace FSOManagement.Util.DLLLoader
{
    public interface IDynamicLibraryLoader : IDisposable
    {
        string LibraryExtension { get; }

        bool LoadLibrary(string name);

        T LoadFunction<T>(string name) where T : class;
    }
}