using Quobject.SocketIoClientDotNet.Modules;
using Xunit;

namespace SocketIoClientDotNet.Tests.ModuleTests
{
    public class HasBinaryDataTest
    {
        [Fact]
        public void ByteArray()
        {
            Assert.True(HasBinaryData.HasBinary(new byte[0]));
        }

        //[Fact]
        //public void ArrayContainsByteArray()
        //{
        //    var arr = JArray.Parse(@"[1, null, 2]");
        //    var bytes = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
        //    var token = JToken.FromObject(bytes);
        //    arr.Add(token);
        //    Assert.True(HasBinaryData.HasBinary(arr));
        //}

    }
}
