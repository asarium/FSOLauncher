#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace FSOManagement.Util.DLLLoader
{
    public class WindowsLoader : IDynamicLibraryLoader
    {
        private IntPtr _handle;

        #region IDynamicLibraryLoader Members

        public void Dispose()
        {
            if (_handle == IntPtr.Zero)
            {
                return;
            }

            FreeLibrary(_handle);
            _handle = IntPtr.Zero;
        }

        string IDynamicLibraryLoader.LibraryExtension
        {
            get { return "dll"; }
        }

        bool IDynamicLibraryLoader.LoadLibrary(string name)
        {
            if (_handle != IntPtr.Zero)
            {
                throw new InvalidOperationException("A library may only be loaded once!");
            }

            _handle = LoadLibrary(name);

            var bit = Environment.Is64BitProcess;

            return _handle != IntPtr.Zero;
        }

        T IDynamicLibraryLoader.LoadFunction<T>(string name)
        {
            var func = GetProcAddress(_handle, name);

            if (func == IntPtr.Zero)
            {
                throw new Exception("Failed to load function " + name);
            }

            return Marshal.GetDelegateForFunctionPointer(func, typeof(T)) as T;
        }

        #endregion

        ~WindowsLoader()
        {
            Dispose();
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        private static extern int FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr handle, string procedureName);
    }
}
