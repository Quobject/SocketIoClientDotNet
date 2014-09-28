using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Parser;

namespace SocketIoClientDotNet.Tests.windowsphone8.TestApp.ParserTests
{
    public class Helpers
    {
        private static Parser.Encoder Encoder = new Parser.Encoder();

        public static void Test(Packet obj)
        {
            Encoder.Encode(obj, new Parser.Encoder.CallbackImp((encodedPackets) =>
            {
                var decoder = new Parser.Decoder();
                decoder.On(Parser.Decoder.EVENT_DECODED, new ListenerImpl((data1) =>
                {
                    var packet = (Packet)data1;
                    AssertPacket(obj, packet);
                }));
                decoder.Add((string)encodedPackets[0]);
            }));
        }

        public static void TestBin(Packet obj)
        {
            object originalData = obj.Data;
            Encoder.Encode(obj, new Parser.Encoder.CallbackImp((encodedPackets) =>
            {
                var decoder = new Parser.Decoder();
                decoder.On(Parser.Decoder.EVENT_DECODED, new ListenerImpl((args) =>
                {
                    var packet = (Packet)args;
                    obj.Data = originalData;
                    obj.Attachments = -1;
                    AssertPacket(obj, packet);
                }));

                foreach (var packet in encodedPackets)
                {
                    if (packet is string)
                    {
                        decoder.Add((string)packet);
                    }
                    else if (packet is byte[])
                    {
                        decoder.Add((byte[])packet);
                    }
                }
            }));
        }


        public static void AssertPacket(Packet expected, Packet actual)
        {
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Nsp, actual.Nsp);
            Assert.AreEqual(expected.Attachments, actual.Attachments);

            if (expected.Data is JArray)
            {
                var exp = (JArray)expected.Data;
                var act = (JArray)expected.Data;
                Assert.AreEqual(exp.ToString(), act.ToString());
            }
            else if (expected.Data is JObject)
            {
                var exp = (JObject)expected.Data;
                var act = (JObject)expected.Data;
                Assert.AreEqual(exp.ToString(), act.ToString());
            }
            else
            {
                Assert.AreEqual(expected.Data.ToString(), actual.Data.ToString());
            }
        }
    }
}
