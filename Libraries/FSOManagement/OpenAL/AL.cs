#region Usings

using System;
using System.Runtime.InteropServices;
using FSOManagement.Util.DLLLoader;

#endregion

namespace FSOManagement.OpenAL
{
    internal class AL
    {
        #region Constants

        public const int AL_VERSION = 0xB002;

        public const int ALC_DEVICE_SPECIFIER = 0x1005;

        public const int ALC_CAPTURE_DEVICE_SPECIFIER = 0x310;

        public const int ALC_ALL_DEVICES_SPECIFIER = 0x1013;

        public const int ALC_DEFAULT_DEVICE_SPECIFIER = 0x1004;

        public const int ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER = 0x311;

        public const int ALC_DEFAULT_ALL_DEVICES_SPECIFIER = 0x1012;

        #endregion

        #region Nested type: CloseDeviceType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CloseDeviceType(IntPtr handle);

        #endregion

        #region Nested type: CreateContextType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateContextType(IntPtr device, IntPtr attrList);

        #endregion

        #region Nested type: DestroyContextType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DestroyContextType(IntPtr context);

        #endregion

        #region Nested type: GetDeviceStringType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetDeviceStringType(IntPtr device, int name);

        #endregion

        #region Nested type: GetErrorType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetErrorType();

        #endregion

        #region Nested type: GetStringType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
        private delegate string GetStringType(int name);

        #endregion

        #region Nested type: IsExtensionPresentType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool IsExtensionPresentType(IntPtr device, string extension);

        #endregion

        #region Nested type: MakeContextCurrentType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool MakeContextCurrentType(IntPtr context);

        #endregion

        #region Nested type: OpenDeviceType

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OpenDeviceType(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))] string deviceName);

        #endregion

        #region Fields

        private CloseDeviceType CloseDeviceFunc;

        private CreateContextType CreateContextFunc;

        private DestroyContextType DestroyContextFunc;

        private GetDeviceStringType GetDeviceStringFunc;

        private GetErrorType GetErrorFunc;

        private GetStringType GetStringFunc;

        private IsExtensionPresentType IsExtensionPresentFunc;

        private MakeContextCurrentType MakeContextCurrentFunc;

        private OpenDeviceType OpenDeviceFunc;

        #endregion

        #region Methods

        public int GetError()
        {
            return GetErrorFunc();
        }

        public string GetString(int name)
        {
            return GetStringFunc(name);
        }

        public bool CloseDevice(IntPtr handle)
        {
            return CloseDeviceFunc(handle);
        }

        public IntPtr CreateContext(IntPtr device, IntPtr attrList)
        {
            return CreateContextFunc(device, attrList);
        }

        public void DestroyContext(IntPtr context)
        {
            DestroyContextFunc(context);
        }

        public IntPtr GetString(IntPtr device, int name)
        {
            return GetDeviceStringFunc(device, name);
        }

        public bool IsExtensionPresent(IntPtr device, string extension)
        {
            return IsExtensionPresentFunc(device, extension);
        }

        public bool MakeContextCurrent(IntPtr context)
        {
            return MakeContextCurrentFunc(context);
        }

        public IntPtr OpenDevice(string deviceName)
        {
            return OpenDeviceFunc(deviceName);
        }

        #endregion

        #region Function pointer loading

        private static void LoadFunction<T>(IDynamicLibraryLoader loader, string name, ref T target) where T : class
        {
            target = loader.LoadFunction<T>(name);
        }

        public void LoadFunctionPointers(IDynamicLibraryLoader loader)
        {
            LoadFunction(loader, "alcGetString", ref GetDeviceStringFunc);
            LoadFunction(loader, "alcIsExtensionPresent", ref IsExtensionPresentFunc);
            LoadFunction(loader, "alGetString", ref GetStringFunc);
            LoadFunction(loader, "alGetError", ref GetErrorFunc);
            LoadFunction(loader, "alcOpenDevice", ref OpenDeviceFunc);
            LoadFunction(loader, "alcCloseDevice", ref CloseDeviceFunc);
            LoadFunction(loader, "alcCreateContext", ref CreateContextFunc);
            LoadFunction(loader, "alcMakeContextCurrent", ref MakeContextCurrentFunc);
            LoadFunction(loader, "alcDestroyContext", ref DestroyContextFunc);
        }

        #endregion
    }
}
