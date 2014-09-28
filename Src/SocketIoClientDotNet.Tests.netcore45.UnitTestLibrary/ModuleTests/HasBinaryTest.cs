using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.SocketIoClientDotNet.Modules;

namespace SocketIoClientDotNet.Tests.netcore45.UnitTestLibrary.ModuleTests
{
    [TestClass]
    public class HasBinaryDataTest
    {
        [TestMethod]
        public void ByteArray()
        {
            Assert.IsTrue(HasBinaryData.HasBinary(new byte[0]));
        }

 

    }
}
