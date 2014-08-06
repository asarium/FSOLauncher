using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace FSOManagement.Tests.Util
{
    public static class SerializationAssert
    {
        public static void IsSerializable<T>(T obj)
        {
            if (!typeof(T).IsSerializable)
            {
                Assert.Fail("Type if not marked as serializable!");
            }
            else
            {
                var formatter = new BinaryFormatter();

                using (var stream = new MemoryStream())
                {
                    Assert.DoesNotThrow(() => formatter.Serialize(stream, obj));
                }
            }
        }
    }
}