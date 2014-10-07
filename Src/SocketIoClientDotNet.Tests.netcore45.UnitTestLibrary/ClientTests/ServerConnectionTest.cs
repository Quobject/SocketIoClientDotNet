using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.SocketIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Parser;
using Socket = Quobject.SocketIoClientDotNet.Client.Socket;

namespace SocketIoClientDotNet.Tests.netcore45.UnitTestLibrary.ClientTests
{
    [TestClass]
    public class ServerConnectionTest : Connection
    {
        private ManualResetEvent ManualResetEvent = null;
        private Socket socket;
        public string Message;
        private int Number;
        private bool Flag;

        [TestMethod]
        public void Connect()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                log.Info("EVENT_CONNECT");
                socket.Disconnect();
            });

            socket.On(Socket.EVENT_DISCONNECT,
                (data) =>
                {
                    log.Info("EVENT_DISCONNECT");
                    Message = (string) data;
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            Assert.AreEqual("io client disconnect", this.Message);
        }



        [TestMethod]
        public void MessageTest()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var count = new[] {0};
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                log.Info("EVENT_CONNECT");
                socket.Emit("hi");
            });

            socket.On("hi",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE");
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = events.Dequeue();
            Assert.AreEqual("more data", (string) obj);
        }


        [TestMethod]
        public void ShouldNotConnectWhenAutoconnectOptionSetToFalse()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);

            var options = CreateOptions();
            options.AutoConnect = false;
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            Assert.IsNull(socket.Io().EngineSocket);
        }

        [TestMethod]
        public void ShouldWorkWithAcks()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);


            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.Emit("ack");

            socket.On("ack", (cb) =>
            {
                var obj = new JObject();
                obj["b"] = true;
                var iack = (IAck) cb;
                iack.Call(5, obj);
            });

            socket.On("got it",
                (data) =>
                {
                    log.Info("got it");
                    ManualResetEvent.Set();
                });

            ManualResetEvent.WaitOne();
            socket.Close();

        }

        [TestMethod]
        public void ShouldReceiveDateWithAck()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            Message = "";
            Number = 0;
            ManualResetEvent = new ManualResetEvent(false);
            var obj = new JObject();
            obj["b"] = true;

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.Emit("getAckDate", new AckImpl((date, n) =>
            {
                log.Info("getAckDate data=" + date);
                Message = ((DateTime) date).ToString("O");
                Number = int.Parse(n.ToString());
                ManualResetEvent.Set();
            }), obj);


            ManualResetEvent.WaitOne();
            Assert.AreEqual(28, Message.Length);
            Assert.AreEqual(5, Number);
            socket.Close();
        }

        [TestMethod]
        public void ShouldReceiveDateWithAckAsAction()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            Message = "";
            Number = 0;
            ManualResetEvent = new ManualResetEvent(false);
            var obj = new JObject();
            obj["b"] = true;

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.Emit("getAckDate", (date, n) =>
            {
                log.Info("getAckDate data=" + date);
                Message = ((DateTime) date).ToString("O");
                Number = int.Parse(n.ToString());
                ManualResetEvent.Set();
            }, obj);


            ManualResetEvent.WaitOne();
            Assert.AreEqual(28, Message.Length);
            Assert.AreEqual(5, Number);
            socket.Close();
        }


        [TestMethod]
        public void ShouldWorkWithFalse()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                socket.Emit("false");
            });

            socket.On("false",
                (data) =>
                {
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = events.Dequeue();
            Assert.IsFalse((bool) obj);
        }


        [TestMethod]
        public void ShouldReceiveUtf8MultibyteCharacters()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var correct = new string[]
            {
                "てすと",
                "Я Б Г Д Ж Й",
                "Ä ä Ü ü ß",
                "utf8 — string",
                "utf8 — string"
            };
            var i = 0;

            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);
            var uri = CreateUri();
            socket = IO.Socket(uri, options);

            socket.On("takeUtf8", (data) =>
            {
                events.Enqueue(data);
                i++;
                log.Info(string.Format("takeUtf8 data={0} i={1}", (string) data, i));

                if (i >= correct.Length)
                {
                    ManualResetEvent.Set();
                }
            });


            socket.Emit("getUtf8");
            ManualResetEvent.WaitOne();
            socket.Close();
            var j = 0;
            foreach (var obj in events)
            {
                var str = (string) obj;
                Assert.AreEqual(correct[j++], str);
            }
        }


        //[TestMethod]
        //public void ShouldReceiveUtf8MultibyteCharactersOverWebsocket()
        //{
        //    var log = LogManager.GetLogger(Global.CallerName());
        //    log.Info("Start");
        //    ManualResetEvent = new ManualResetEvent(false);
        //    var events = new Queue<object>();

        //    var correct = new string[]
        //    {
        //        "てすと",
        //        "Я Б Г Д Ж Й",
        //        "Ä ä Ü ü ß",
        //        "utf8 — string",
        //        "utf8 — string"
        //    };
        //    var i = 0;

        //    var options = CreateOptions();
        //    var uri = CreateUri();
        //    socket = IO.Socket(uri, options);

        //    socket.On("takeUtf8", (data) =>
        //    {
        //        events.Enqueue(data);
        //        i++;
        //        log.Info(string.Format("takeUtf8 data={0} i={1}", (string)data, i));

        //        if (i >= correct.Length)
        //        {
        //            ManualResetEvent.Set();
        //        }
        //    });


        //    socket.Emit("getUtf8");
        //    ManualResetEvent.WaitOne();
        //    socket.Close();
        //    var j = 0;
        //    foreach (var obj in events)
        //    {
        //        var str = (string)obj;
        //        Assert.AreEqual(correct[j++], str);
        //    }
        //}

        //[TestMethod]
        //public void ShouldConnectToANamespaceAfterConnectionEstablished()
        //{
        //    var log = LogManager.GetLogger(Global.CallerName());
        //    log.Info("Start");
        //    ManualResetEvent = new ManualResetEvent(false);
        //    Flag = false;

        //    var options = CreateOptions();
        //    var uri = CreateUri();

        //    var manager = new Manager( new Uri(uri), options);
        //    socket = manager.Socket("/");

        //    socket.On(Socket.EVENT_CONNECT, () =>
        //    {
        //        var foo = manager.Socket("/foo");
        //        foo.On(Socket.EVENT_CONNECT, () =>
        //        {
        //            Flag = true;
        //            foo.Disconnect();
        //            socket.Disconnect();
        //            manager.Close();
        //            ManualResetEvent.Set();
        //        });
        //        foo.Open();
        //    });


        //    ManualResetEvent.WaitOne();
        //    Assert.IsTrue(Flag);
        //}

        [TestMethod]
        public void ShouldOpenANewNamespaceAfterConnectionGetsClosed()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var options = CreateOptions();
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/");

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                socket.Disconnect();
            });

            socket.On(Socket.EVENT_DISCONNECT, () =>
            {
                var foo = manager.Socket("/foo");
                foo.On(Socket.EVENT_CONNECT, () =>
                {
                    Flag = true;
                    foo.Disconnect();
                    socket.Disconnect();
                    manager.Close();
                    ManualResetEvent.Set();
                });
                foo.Open();
            });
            ManualResetEvent.WaitOne();
            Assert.IsTrue(Flag);
        }



        [TestMethod]
        public void ReconnectEventShouldFireInSocket()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var options = CreateOptions();
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/");

            socket.On(Socket.EVENT_RECONNECT, () =>
            {
                log.Info("EVENT_RECONNECT");
                Flag = true;

                ManualResetEvent.Set();
            });

            Task.Delay(2000).Wait();
            manager.EngineSocket.Close();
            ManualResetEvent.WaitOne();
            log.Info("before EngineSocket close");




            Assert.IsTrue(Flag);
        }

        [TestMethod]
        public async Task ShouldReconnectByDefault()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var options = CreateOptions();
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/");

            manager.On(Socket.EVENT_RECONNECT, () =>
            {
                log.Info("EVENT_RECONNECT");
                Flag = true;
                socket.Disconnect();
                ManualResetEvent.Set();
            });

            await Task.Delay(500);
            log.Info("before EngineSocket close");
            manager.EngineSocket.Close();

            ManualResetEvent.WaitOne();
            Assert.IsTrue(Flag);
        }

        [TestMethod]
        public void ShouldTryToReconnectTwiceAndFailWhenRequestedTwoAttemptsWithImmediateTimeoutAndReconnectEnabled()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;
            var reconnects = 0;

            var options = CreateOptions();
            options.Reconnection = true;
            options.Timeout = 0;
            options.ReconnectionAttempts = 2;
            options.ReconnectionDelay = 10;
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);

            manager.On(Manager.EVENT_RECONNECT_ATTEMPT, () =>
            {
                log.Info("EVENT_RECONNECT_ATTEMPT");
                reconnects++;
            });

            manager.On(Manager.EVENT_RECONNECT_FAILED, () =>
            {
                log.Info("EVENT_RECONNECT_FAILED");
                Flag = true;
                manager.Close();
                ManualResetEvent.Set();
            });

            ManualResetEvent.WaitOne();
            Assert.IsTrue(Flag);
            Assert.AreEqual(2, reconnects);
        }

        [TestMethod]
        public void ShouldFireReconnectEventsOnSocket()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;
            var reconnects = 0;
            var events = new Queue<int>();

            var correct = new int[]
            {
                1, 2, 2
            };

            var options = CreateOptions();
            options.Reconnection = true;
            options.Timeout = 0;
            options.ReconnectionAttempts = 2;
            options.ReconnectionDelay = 10;
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/timeout_socket");

            socket.On(Socket.EVENT_RECONNECT_ATTEMPT, (attempts) =>
            {
                log.Info("EVENT_RECONNECT_ATTEMPT");
                reconnects++;
                events.Enqueue(int.Parse((attempts).ToString()));
            });

            socket.On(Socket.EVENT_RECONNECT_FAILED, () =>
            {
                log.Info("EVENT_RECONNECT_FAILED");
                Flag = true;
                events.Enqueue(reconnects);
                socket.Close();
                manager.Close();
                ManualResetEvent.Set();
            });

            ManualResetEvent.WaitOne();
            var j = 0;
            foreach (var number in events)
            {
                Assert.AreEqual(correct[j++], number);
            }
        }


        [TestMethod]
        public void ShouldFireErrorOnSocket()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;
            var events = new Queue<object>();

            var options = CreateOptions();
            options.Reconnection = true;
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/timeout_socket");

            socket.On(Socket.EVENT_ERROR, (e) =>
            {
                var exception = (EngineIOException) e;
                log.Info("EVENT_ERROR");
                events.Enqueue(exception.code);
                socket.Close();
                manager.Close();
                ManualResetEvent.Set();
            });

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                log.Info("EVENT_CONNECT");
                manager.EngineSocket.OnPacket(new Packet(Packet.ERROR, "test"));
            });

            ManualResetEvent.WaitOne();
            var obj = (string) events.Dequeue();
            Assert.AreEqual("test", obj);
        }

        [TestMethod]
        public void ShouldFireReconnectingOnSocketWithAttemptsNumberWhenReconnectingTwice()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;
            var reconnects = 0;
            var events = new Queue<int>();

            var correct = new int[]
            {
                1, 2, 2
            };

            var options = CreateOptions();
            options.Reconnection = true;
            options.Timeout = 0;
            options.ReconnectionAttempts = 2;
            options.ReconnectionDelay = 10;
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/timeout_socket");

            socket.On(Socket.EVENT_RECONNECTING, (attempts) =>
            {
                reconnects++;
                events.Enqueue(int.Parse((attempts).ToString()));
            });

            socket.On(Socket.EVENT_RECONNECT_FAILED, () =>
            {
                log.Info("EVENT_RECONNECT_FAILED");
                Flag = true;
                events.Enqueue(reconnects);
                socket.Close();
                manager.Close();
                ManualResetEvent.Set();
            });

            ManualResetEvent.WaitOne();
            var j = 0;
            foreach (var number in events)
            {
                Assert.AreEqual(correct[j++], number);
            }
        }



        [TestMethod]
        public void ShouldNotTryToReconnectAndShouldFormAConnectionWhenConnectingToCorrectPortWithDefaultTimeout()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;


            var options = CreateOptions();
            options.Reconnection = true;
            options.ReconnectionDelay = 10;
            var uri = CreateUri();

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/valid");

            manager.On(Manager.EVENT_RECONNECT_ATTEMPT, () =>
            {
                Flag = true;
            });

            socket.On(Socket.EVENT_CONNECT, async () =>
            {
                // set a timeout to let reconnection possibly fire
                log.Info("EVENT_CONNECT");
                await Task.Delay(1000);

                ManualResetEvent.Set();
            });

            ManualResetEvent.WaitOne();
            log.Info("after WaitOne");
            socket.Close();
            manager.Close();
            Assert.IsFalse(Flag);
        }

        [TestMethod]
        public void ShouldTryToReconnectTwiceAndFailWhenRequestedTwoAttemptsWithIncorrectAddressAndReconnectEnabled()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;
            var reconnects = 0;

            var options = CreateOptions();
            options.Reconnection = true;
            options.ReconnectionAttempts = 2;
            options.ReconnectionDelay = 10;
            var uri = "http://localhost:3940";


            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/asd");

            manager.On(Manager.EVENT_RECONNECT_ATTEMPT, () =>
            {
                log.Info("EVENT_RECONNECT_ATTEMPT");
                reconnects++;
            });

            manager.On(Manager.EVENT_RECONNECT_FAILED, () =>
            {
                log.Info("EVENT_RECONNECT_FAILED");
                Flag = true;
                socket.Disconnect();
                manager.Close();
                ManualResetEvent.Set();
            });

            ManualResetEvent.WaitOne();
            Assert.AreEqual(2, reconnects);
        }

        [TestMethod]
        public void ShouldNotTryToReconnectWithIncorrectPortWhenReconnectionDisabled()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;


            var options = CreateOptions();
            options.Reconnection = false;
            var uri = "http://localhost:3940";

            var manager = new Manager(new Uri(uri), options);
            socket = manager.Socket("/invalid");

            manager.On(Manager.EVENT_RECONNECT_ATTEMPT, () =>
            {
                Flag = true;
            });

            manager.On(Manager.EVENT_CONNECT_ERROR, async () =>
            {
                // set a timeout to let reconnection possibly fire
                log.Info("EVENT_CONNECT_ERROR");
                await Task.Delay(1000);
                ManualResetEvent.Set();
            });

            ManualResetEvent.WaitOne();
            log.Info("after WaitOne");
            socket.Disconnect();
            manager.Close();
            Assert.IsFalse(Flag);
        }

        [TestMethod]
        public void ShouldEmitDateAsDate()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("takeDate", (data) =>
            {
                log.Info("takeDate");
                events.Enqueue(data);
                ManualResetEvent.Set();
            });

            socket.Emit("getDate");

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = events.Dequeue();
            Assert.IsInstanceOfType(obj, typeof(DateTime) );
        }



        [TestMethod]
        public void ShouldEmitDateInObject()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("takeDateObj", (data) =>
            {
                log.Info("takeDate");
                events.Enqueue(data);
                ManualResetEvent.Set();
            });

            socket.Emit("getDateObj");

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = (JObject) events.Dequeue();

            Assert.IsInstanceOfType(obj, typeof(JObject));

            var date = (obj["date"]).Value<DateTime>();
            Assert.IsInstanceOfType(date, typeof(DateTime));

        }

        [TestMethod]
        public void ShouldGetBase64DataAsALastResort()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("takebin", (data) =>
            {
                events.Enqueue(data);
                ManualResetEvent.Set();
            });

            socket.Emit("getbin");

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();

            var binData = (byte[]) events.Dequeue();
            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
            var i = 0;
            foreach (var b in exptected)
            {
                Assert.AreEqual(b, binData[i++]);
            }

        }

        [TestMethod]
        public void ShouldGetBinaryDataAsAnArraybuffer()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("doge", (data) =>
            {
                events.Enqueue(data);
                ManualResetEvent.Set();
            });

            socket.Emit("doge");

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();

            var binData = (byte[]) events.Dequeue();
            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
            var i = 0;
            foreach (var b in exptected)
            {
                Assert.AreEqual(b, binData[i++]);
            }
        }

        [TestMethod]
        public void ShouldSendBinaryDataAsAnArraybuffer()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("buffack", () =>
            {
                Flag = true;
                ManualResetEvent.Set();
            });

            socket.Emit("buffa", exptected);

            ManualResetEvent.WaitOne();
            socket.Close();
            Assert.IsTrue(Flag);
        }

        [TestMethod]
        public void BuffAck()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
            var events = new Queue<object>();

            var options = CreateOptions();
            //options.Transports = ImmutableList.Create<string>(Polling.NAME);
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("buffack", () =>
            {
                Flag = true;
                ManualResetEvent.Set();
            });

            socket.Emit("buffa", exptected);

            ManualResetEvent.WaitOne();
            //Task.Delay(8000).Wait();
            socket.Close();
            //Task.Delay(4000).Wait();
            Assert.IsTrue(Flag);
            log.Info("Finish");
        }

        [TestMethod]
        public void DoubleCallTest()
        {
            ShouldSendBinaryDataAsAnArraybufferMixedWithJson();
            ShouldSendBinaryDataAsAnArraybufferMixedWithJson();
            ShouldSendBinaryDataAsAnArraybufferMixedWithJson();

        }

        [TestMethod]
        public void ShouldSendBinaryDataAsAnArraybufferMixedWithJson()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var buf = System.Text.Encoding.UTF8.GetBytes("howdy");
            var jobj = new JObject();
            jobj.Add("hello", "lol");
            jobj.Add("message", buf);
            jobj.Add("goodbye", "gotcha");

            var options = CreateOptions();
            //options.Transports = ImmutableList.Create<string>(Polling.NAME);

            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("jsonbuff-ack", () =>
            {
                Flag = true;
                ManualResetEvent.Set();
            });

            socket.On(Socket.EVENT_DISCONNECT, () =>
            {
                log.Info("EVENT_DISCONNECT");
            });

            socket.Emit("jsonbuff", jobj);

            ManualResetEvent.WaitOne();
            Task.Delay(1).Wait();

            log.Info("About to wait 1sec");

            //Task.Delay(1000).Wait();
            log.Info("About to call close");
            socket.Close();
            //Task.Delay(1000).Wait();
            Assert.IsTrue(Flag);
            log.Info("Finish");
        }

        [TestMethod]
        public async Task ShouldSendEventsWithArraybuffersInTheCorrectOrder()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;
            var buf = System.Text.Encoding.UTF8.GetBytes("abuff1");

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On("abuff2-ack", () =>
            {
                Flag = true;
                ManualResetEvent.Set();
            });


            await Task.Delay(5000);
            socket.Emit("abuff1", buf);
            socket.Emit("abuff2", "please arrive second");
            ManualResetEvent.WaitOne();
            Assert.IsTrue(Flag);
        }


    }
}

