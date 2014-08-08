#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FSOManagement.Util.DLLLoader;

#endregion

namespace FSOManagement.OpenAL
{
    public class OpenALManager
    {
        #region DeviceType enum

        public enum DeviceType
        {
            Playback,

            Capture
        }

        #endregion

        private static readonly string[] LibraryNames = {"OpenAL32", "libopenal", "OpenAL"};

        private OpenALManager()
        {
        }

        private static IDynamicLibraryLoader LoadOpenALLibrary(string searchPath)
        {
            IDynamicLibraryLoader loader;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                loader = new WindowsLoader();
            }
            else
            {
                loader = new LinuxLoader();
            }

            // First try to load the library from the search path
            if (
                LibraryNames.Select(libraryName => Path.Combine(searchPath, libraryName + "." + loader.LibraryExtension))
                    .FirstOrDefault(path => loader.LoadLibrary(path)) != null)
            {
                return loader;
            }

            // Then load the system library
            if (LibraryNames.FirstOrDefault(libraryName => loader.LoadLibrary(libraryName + "." + loader.LibraryExtension)) != null)
            {
                return loader;
            }

            throw new Exception("Failed to load OpenAL library!");
        }

        public static IEnumerable<string> GetDevices(DeviceType deviceType, string librarySearchPath)
        {
            using (var loader = LoadOpenALLibrary(librarySearchPath))
            {
                var al = new AL();
                al.LoadFunctionPointers(loader);

                if (!al.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATION_EXT"))
                {
                    return null;
                }
                int type;

                switch (deviceType)
                {
                    case DeviceType.Playback:
                        type = AL.ALC_DEVICE_SPECIFIER;
                        if (al.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATE_ALL_EXT"))
                        {
                            type = AL.ALC_ALL_DEVICES_SPECIFIER;
                        }
                        break;
                    case DeviceType.Capture:
                        type = AL.ALC_CAPTURE_DEVICE_SPECIFIER;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("deviceType");
                }

                var devices = al.GetString(IntPtr.Zero, type);

                if (devices == IntPtr.Zero)
                {
                    return Enumerable.Empty<string>();
                }

                var data = new List<string>();
                byte[] buffer;
                do
                {
                    var stringBuffer = Marshal.PtrToStringAnsi(devices);

                    // Assume that the length is the same as the length of the byte array
                    // which should be the case as ANSI is used above

                    buffer = new byte[stringBuffer.Length];
                    Marshal.Copy(devices, buffer, 0, stringBuffer.Length);

                    devices += buffer.Length + 1;

                    if (buffer.Length > 0)
                    {
                        data.Add(Encoding.UTF8.GetString(buffer));
                    }
                } while (buffer.Length > 0);

                return data;
            }
        }

        public static string GetDefaultDevice(DeviceType deviceType, string librarySearchPath)
        {
            using (var loader = LoadOpenALLibrary(librarySearchPath))
            {
                var al = new AL();
                al.LoadFunctionPointers(loader);

                if (!al.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATION_EXT"))
                {
                    return null;
                }
                int type;

                switch (deviceType)
                {
                    case DeviceType.Playback:
                        type = AL.ALC_DEFAULT_DEVICE_SPECIFIER;
                        if (al.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATE_ALL_EXT"))
                        {
                            type = AL.ALC_DEFAULT_ALL_DEVICES_SPECIFIER;
                        }
                        break;
                    case DeviceType.Capture:
                        type = AL.ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("deviceType");
                }

                var device = al.GetString(IntPtr.Zero, type);

                if (device == IntPtr.Zero)
                {
                    return null;
                }

                var stringBuffer = Marshal.PtrToStringAnsi(device);

                // Assume that the length is the same as the length of the byte array
                // which should be the case as ANSI is used above
                var buffer = new byte[stringBuffer.Length];
                Marshal.Copy(device, buffer, 0, stringBuffer.Length);

                return buffer.Length > 0 ? Encoding.UTF8.GetString(buffer) : null;
            }
        }

        public static DeviceProperties GetDeviceProperties(string deviceName, string librarySearchPath)
        {
            using (var loader = LoadOpenALLibrary(librarySearchPath))
            {
                var al = new AL();
                al.LoadFunctionPointers(loader);

                var device = al.OpenDevice(deviceName);

                if (device == IntPtr.Zero)
                {
                    throw new Exception("Failed to open device!");
                }

                try
                {
                    var hasEfx = al.IsExtensionPresent(device, "ALC_EXT_EFX");

                    var context = al.CreateContext(device, IntPtr.Zero);

                    if (context == IntPtr.Zero)
                    {
                        throw new Exception("Failed to create context!");
                    }

                    try
                    {
                        if (!al.MakeContextCurrent(context))
                        {
                            throw new Exception("Failed to make context current!");
                        }

                        var version = al.GetString(AL.AL_VERSION);

                        return new DeviceProperties(version, hasEfx);
                    }
                    finally
                    {
                        al.DestroyContext(context);
                    }
                }
                finally
                {
                    al.MakeContextCurrent(IntPtr.Zero);
                    al.CloseDevice(device);
                    device = IntPtr.Zero;
                }
            }
        }

        #region Nested type: DeviceProperties

        public class DeviceProperties
        {
            public DeviceProperties(string version, bool supportsEfx)
            {
                Version = version;
                SupportsEfx = supportsEfx;
            }

            public string Version { get; private set; }

            public bool SupportsEfx { get; private set; }
        }

        #endregion
    }
}
