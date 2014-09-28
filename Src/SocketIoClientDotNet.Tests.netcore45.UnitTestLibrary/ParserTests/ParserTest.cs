using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Parser;

namespace SocketIoClientDotNet.Tests.netcore45.UnitTestLibrary.ParserTests
{
    [TestClass]
    public class ParserTest
    {
        private static Parser.Encoder _encoder = new Parser.Encoder();


        [TestMethod]
        public void Decode()
        {
            var decoder = new Parser.Decoder();
            var called = false;
            decoder.On(Parser.Decoder.EVENT_DECODED, new ListenerImpl((data1) =>
            {
                var packet = (Packet)data1;
                called = true;

            }));
            decoder.Add("0/woot");
            Assert.IsTrue(called);
        }


        [TestMethod]
        public void EncodeConnection()
        {
            var packet = new Packet(Parser.CONNECT) { Nsp = "/woot" };
            Helpers.Test(packet);
        }

        [TestMethod]
        public void EncodeDisconnection()
        {
            var packet = new Packet(Parser.DISCONNECT) { Nsp = "/woot" };
            Helpers.Test(packet);
        }

        [TestMethod]
        public void EncodeEvent()
        {
            var packet = new Packet(Parser.EVENT) { Nsp = "/", Data = JArray.Parse("[\"a\", 1, {}]") };
            Helpers.Test(packet);

            var packet2 = new Packet(Parser.EVENT) { Nsp = "/test", Data = JArray.Parse("[\"a\", 1, {}]") };
            Helpers.Test(packet2);

        }

        [TestMethod]
        public void EncodeAck()
        {
            var packet = new Packet(Parser.ACK) { Id = 123, Nsp = "/", Data = JArray.Parse("[\"a\", 1, {}]") };
            Helpers.Test(packet);
        }


    }
}
