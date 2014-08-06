using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FSOManagement.Util;

namespace FSOManagement
{
    public static class FlagFileReader
    {
        public static async Task<BuildCapabilities> ReadFlagFileAsync(Stream fileStream)
        {
            var buildCaps = new BuildCapabilities();

            using (var binaryReader = new BinaryReader(fileStream, Encoding.UTF8, true))
            {
                var easyFlagSize = binaryReader.ReadInt32();
                var flagsSize = binaryReader.ReadInt32();

                var easyFlagSizeof = Marshal.SizeOf(typeof(Util.EasyFlag));
                var flagSizeOf = Marshal.SizeOf(typeof(Util.Flag));

                if (easyFlagSize != easyFlagSizeof)
                {
                    throw new NotSupportedException("The size of the easy flag data structure doesn't match!");
                }

                if (flagsSize != flagSizeOf)
                {
                    throw new NotSupportedException("The size of the flag data structure doesn't match!");
                }

                var numEasyFlags = binaryReader.ReadInt32();

                for (var i = 0; i < numEasyFlags; ++i)
                {
                    buildCaps.AddEasyFlag(EasyFlag.CopyFromStruct(await fileStream.ReadStructAsync<Util.EasyFlag>()));
                }

                var numFlags = binaryReader.ReadInt32();

                for (var i = 0; i < numFlags; ++i)
                {
                    buildCaps.AddFlag(Flag.CopyFromStruct(await fileStream.ReadStructAsync<Util.Flag>()));
                }

                // Stupid way of getting the enum value from a byte...
                buildCaps.BuildCaps = (BuildCapabilitiesFlags) Enum.Parse(typeof(BuildCapabilitiesFlags), binaryReader.ReadByte().ToString());
            }

            return buildCaps;
        }
    }
}