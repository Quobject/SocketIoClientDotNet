using Quobject.SocketIoClientDotNet.Client;
using Xunit;

namespace SocketIoClientDotNet.Tests.ClientTests
{
    public class UrlTest
    {
        [Fact]
        public void Parse()
        {
            const string test = @"http://username:password@host:8080/directory/file?query#ref";
            var result = Url.Parse(test);
            var str = result.ToString();
            Assert.Equal(test,str);
        }

        [Fact]
        public void ParseRelativePath()
        {
            const string test = @"https://woot.com/test";
            var result = Url.Parse(test);
            Assert.Equal("https",result.Scheme);
            Assert.Equal("woot.com",result.Host);
            Assert.Equal("/test",result.LocalPath);
        }

        [Fact]
        public void ParseNoProtocol()
        {
            const string test = @"//localhost:3000";
            var result = Url.Parse(test);
            Assert.Equal("http", result.Scheme);
            Assert.Equal("localhost", result.Host);
            Assert.Equal(3000, result.Port);
        }

        [Fact]
        public void ParseNamespace()
        {
            var result = Url.Parse(@"http://woot.com/woot");
            Assert.Equal("/woot", result.LocalPath);
            result = Url.Parse(@"http://google.com");
            Assert.Equal("/", result.LocalPath);
            result = Url.Parse(@"http://google.com/");
            Assert.Equal("/", result.LocalPath);
        }

    }
}
