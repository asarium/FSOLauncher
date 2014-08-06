#region Usings

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#endregion

namespace FSOManagement.Util
{
    // Copied from http://stackoverflow.com/questions/4159184/c-read-structures-from-binary-file
    public static class StreamExtensions
    {
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            stream.Read(buffer, 0, sz);

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            var structure = (T) Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(T));

            pinnedBuffer.Free();
            return structure;
        }

        public static async Task<T> ReadStructAsync<T>(this Stream stream) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            await stream.ReadAsync(buffer, 0, sz);

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            var structure = (T) Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(T));

            pinnedBuffer.Free();
            return structure;
        }

        public static void WriteStruct<T>(this Stream stream, T val) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(val, pinnedBuffer.AddrOfPinnedObject(), false);

                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                if (pinnedBuffer != null)
                {
                    pinnedBuffer.Free();
                }
            }
        }

        public static async Task WriteStructAsync<T>(this Stream stream, T val) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(val, pinnedBuffer.AddrOfPinnedObject(), false);

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            finally
            {
                if (pinnedBuffer != null)
                {
                    pinnedBuffer.Free();
                }
            }
        }
    }
}
