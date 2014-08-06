#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace FSOManagement.Util.DLLLoader
{
    public sealed class LinuxLoader : IDynamicLibraryLoader
    {
        private const int RTLD_NOW = 2;

        private IntPtr _handle;

        #region IDynamicLibraryLoader Members

        public T LoadFunction<T>(string name) where T : class
        {
            // clear previous errors if any
            dlerror();
            var res = dlsym(_handle, name);
            var errPtr = dlerror();
            if (errPtr != IntPtr.Zero)
            {
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
            }

            return Marshal.GetDelegateForFunctionPointer(res, typeof(T)) as T;
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero)
            {
                return;
            }

            dlclose(_handle);
            _handle = IntPtr.Zero;
        }

        public string LibraryExtension
        {
            get { return "so"; }
        }

        public bool LoadLibrary(string name)
        {
            _handle = dlopen(name, RTLD_NOW);

            return _handle != IntPtr.Zero;
        }

        #endregion

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(String fileName, int flags);

        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, String symbol);

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();

        ~LinuxLoader()
        {
            Dispose();
        }
    }
}
