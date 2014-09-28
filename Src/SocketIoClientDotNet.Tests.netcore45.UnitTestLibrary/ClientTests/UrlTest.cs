using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.SocketIoClientDotNet.Client;

namespace SocketIoClientDotNet.Tests.netcore45.UnitTestLibrary.ClientTests
{
    [TestClass]
    public class UrlTest
    {
        [TestMethod]
        public void Parse()
        {
            const string test = @"http://username:password@host:8080/directory/file?query#ref";
            var result = Url.Parse(test);
            var str = result.ToString();
            Assert.AreEqual(test, str);
        }

        [TestMethod]
        public void ParseRelativePath()
        {
            const string test = @"https://woot.com/test";
            var result = Url.Parse(test);
            Assert.AreEqual("https", result.Scheme);
            Assert.AreEqual("woot.com", result.Host);
            Assert.AreEqual("/test", result.LocalPath);
        }

        [TestMethod]
        public void ParseNoProtocol()
        {
            const string test = @"//localhost:3000";
            var result = Url.Parse(test);
            Assert.AreEqual("http", result.Scheme);
            Assert.AreEqual("localhost", result.Host);
            Assert.AreEqual(3000, result.Port);
        }

        [TestMethod]
        public void ParseNamespace()
        {
            var result = Url.Parse(@"http://woot.com/woot");
            Assert.AreEqual("/woot", result.LocalPath);
            result = Url.Parse(@"http://google.com");
            Assert.AreEqual("/", result.LocalPath);
            result = Url.Parse(@"http://google.com/");
            Assert.AreEqual("/", result.LocalPath);
        }

    }
}
