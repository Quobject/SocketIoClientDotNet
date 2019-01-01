using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Socket = Quobject.SocketIoClientDotNet.Client.Socket;

namespace SocketIoClientDotNet.Tests.ClientTests
{

    public class ServerConnectionTest : Connection
    {
        private ManualResetEvent ManualResetEvent = null;
        private Socket socket;
        public string Message;
        private int Number;
        private bool Flag;


        [Fact]
        public void OgsMessageTest()
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
                log.Info("EVENT_CONNECT");
                socket.Emit("parser_error#21");
            });

            socket.On("parser_error#21_response",
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
            Assert.Equal("{\"handicap\":0,\"disable_analysis\":false,\"private\":false,\"height\":19,\"time_control\":{\"time_control\":\"byoyomi\",\"period_time\":45,\"main_time\":1800,\"periods\":5},\"ranked\":false,\"meta_groups\":[132,3,196,261,327,489,43,418,302,463,242,403,425,24,25,154,28,157],\"komi\":6.5,\"game_id\":1333070,\"width\":19,\"rules\":\"japanese\",\"black_player_id\":71283,\"white_player_id\":78080,\"players\":{\"white\":{\"username\":\"Rvzy\",\"egf\":501.413,\"rank\":14,\"id\":78080,\"accepted_stones\":\"mcidkdldmdiekeefgfifkfdgegfghgkgfhghhhihkhnhfigihikinifjhjjjkjnjdkekfkgkhkjknkflhljlgmhmimjmgnknlnmnnngo\",\"accepted_strict_seki_mode\":false},\"black\":{\"username\":\"心の光\",\"egf\":353.454,\"rank\":12,\"id\":71283,\"accepted_stones\":\"mcidkdldmdiekeefgfifkfdgegfghgkgfhghhhihkhnhfigihikinifjhjjjkjnjdkekfkgkhkjknkflhljlgmhmimjmgnknlnmnnngo\",\"accepted_strict_seki_mode\":false}},\"game_name\":\"Friendly Match\",\"phase\":\"finished\",\"history\":[],\"initial_player\":\"black\",\"moves\":\"pddpppddfqfocqdjcpcodqepeqncqfkcfcgdcccddcfdecgcgbhbfahcbdbebccfpjnpqnjphphoioipgohngngphqfpgmilcndnbocmdoengqeocodlgkijhiiihhjgifighghfhkikekejfjelflfihjgihminimjnjmkmjlkljkkkjjliihjikjljkijhkhlgkgjfiejegfheidickfmekejdhllhfgehegdhfhedghffeffedgcgdkckfkcibmclblbkldlcmdndmcmbkdjcaeafadnfknjolnlplkllmkmmmnnmnnnkmjnjminimhnhmglfnlomolonmloonglmiqjqjrkrirmfofpbqcorproqpqopplpmqmpoqoosqppsqrqsrrrsoeneodwO6obqblaocnbpaoaqaksjsakalhafbsssr....\",\"allow_self_capture\":false,\"automatic_stone_removal\":false,\"free_handicap_placement\":false,\"allow_ko\":false,\"allow_superko\":true,\"superko_algorithm\":\"ssk\",\"score_territory\":true,\"score_territory_in_seki\":false,\"score_stones\":false,\"score_prisoners\":true,\"score_passes\":true,\"white_must_pass_last\":false,\"opponent_plays_first_after_resume\":true,\"strict_seki_mode\":false,\"initial_state\":{\"black\":\"\",\"white\":\"\"},\"start_time\":1420522348,\"clock\":{\"game_id\":1333070,\"current_player\":78080,\"black_player_id\":71283,\"white_player_id\":78080,\"title\":\"Friendly Match\",\"last_move\":1420524830517,\"expiration\":1420525270589,\"black_time\":{\"thinking_time\":909.8829999999991,\"periods\":5,\"period_time\":45},\"white_time\":{\"thinking_time\":215.07199999999966,\"periods\":5,\"period_time\":45},\"pause_delta\":-517,\"expiration_delta\":440072,\"now\":1420524830517,\"paused_since\":1420524830},\"pause_control\":{\"stone-removal\":true},\"paused_since\":1420524830,\"removed\":\"mcidkdldmdiekeefgfifkfdgegfghgkgfhghhhihkhnhfigihikinifjhjjjkjnjdkekfkgkhkjknkflhljlgmhmimjmgnknlnmnnngo\",\"auto_scoring_done\":true,\"score\":{\"white\":{\"total\":154.5,\"stones\":0,\"territory\":101,\"prisoners\":47,\"scoring_positions\":\"iajakaibjbkblbmanamckdldmdcedeeegekeledfefgfkfdgegfggghgkgfhghhhihkhdieifigihikifjgjhjjjkjdkekfkgkhkjkflglhljldmemfmgmhmimjmfngngohdidieifbfagbgahbhchaibiajbjcjknlnmnnnkolomonokpmpkqlqmqnqlrmrnrlsmsnsio\",\"handicap\":0,\"komi\":6.5},\"black\":{\"total\":101,\"stones\":0,\"territory\":94,\"prisoners\":7,\"scoring_positions\":\"aabacadaeaabbbcbdbebacrasarbsbrcscqdrdsdpeqeresepfrfsfogpgqgrgsgnhohphqhrhshnioipiqirisinjojqjrjsjnkokpkqkrkskqlrlslrmsmrnsnrosorpspqqrqsqamanbnaoapbpaqbqarbrcrdrerfrgrhrasbscsdsesfsgshsis\",\"handicap\":0,\"komi\":0}},\"winner\":78080,\"outcome\":\"53.5 points\",\"end_time\":1420524844}", obj);
        }




        [Fact]
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
            Assert.Equal("io client disconnect", this.Message);
        }



        [Fact]
        public void MessageTest()
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
            Assert.Equal("more data", obj);
        }


        [Fact]
        public void ShouldNotConnectWhenAutoconnectOptionSetToFalse()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);

            var options = CreateOptions();
            options.AutoConnect = false;
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            Assert.Null(socket.Io().EngineSocket);
        }

        [Fact]
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



        public class Acks2ListenerImpl : IListener
        {
            private static int id_counter = 0;
            private int Id;
            private readonly Action<object,object> fn;

            public Acks2ListenerImpl(Action<object,object> fn)
            {

                this.fn = fn;
                this.Id = id_counter++;
            }

          

            public void Call(params object[] args)
            {
                var arg1 = args.Length > 0 ? args[0] : null;
                var arg2 = args.Length > 1 ? args[1] : null;

                fn(arg1, arg2);
            }


            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return Id;
            }
        }




        [Fact]
        public void ShouldWorkWithAcks2()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            string message = "";

            var options = CreateOptions();
            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.Emit("ack2");

            var myIListenerImpl = new Acks2ListenerImpl((msg, cb) =>
            {
                message = (string)msg;
                var obj = new JObject();
                obj["b"] = true;
                var iack = (IAck)cb;
                iack.Call(5, obj);

            });


            socket.On("ack2", myIListenerImpl);

            socket.On("got it",
                (data) =>
                {
                    log.Info("got it");
                    ManualResetEvent.Set();
                });

            ManualResetEvent.WaitOne();
            Assert.Equal("hello there", message);
            socket.Close();

        }


        [Fact]
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
            Assert.Equal(28, Message.Length);
            Assert.Equal(5, Number);
            socket.Close();
        }

        [Fact]
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
                Message = ((DateTime)date).ToString("O");
                Number = int.Parse(n.ToString());
                ManualResetEvent.Set();
            }, obj);


            ManualResetEvent.WaitOne();
            Assert.Equal(28, Message.Length);
            Assert.Equal(5, Number);
            socket.Close();
        }


        [Fact]
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
            Assert.False((bool) obj);
        }


        [Fact]
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
                "李O四",
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
                log.Info(string.Format("takeUtf8 data={0} i={1}",(string)data,i));

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
                Assert.Equal(correct[j++], str);
            }
        }


        //[Fact]
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
        //        Assert.Equal(correct[j++], str);
        //    }
        //}

        //[Fact]
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
        //    Assert.True(Flag);
        //}

        [Fact]
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
            Assert.True(Flag);
        }



        [Fact]
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




            Assert.True(Flag);
        }

        [Fact]
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
            Assert.True(Flag);
        }

        [Fact]
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
            Assert.True(Flag);
            Assert.Equal(2,reconnects);
        }

        [Fact]
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
                1,2,2
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
                Assert.Equal(correct[j++], number);
            }            
        }


        [Fact]
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
            var obj = (string)events.Dequeue();
            Assert.Equal("test", obj);
        }

        [Fact]
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
                1,2,2
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
                Assert.Equal(correct[j++], number);
            }               
        }



        [Fact]
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

            socket.On(Socket.EVENT_CONNECT, async() =>
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
            Assert.False(Flag);                   
        }

        [Fact]
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
            Assert.Equal(2, reconnects);
        }

        [Fact]
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
            Assert.False(Flag);     
        }

        [Fact]
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
            Assert.IsType<DateTime>(obj);           
        }



        [Fact]
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

            Assert.IsType<JObject>(obj);
            var date = (obj["date"]).Value<DateTime>();
            Assert.IsType<DateTime>(date);
        }

        [Fact]
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

            var binData = (byte[])events.Dequeue();
            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
            var i = 0;
            foreach (var b in exptected)
            {
                Assert.Equal(b, binData[i++]);
            }
        
        }

        [Fact]
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

            var binData = (byte[])events.Dequeue();
            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");
            var i = 0;
            foreach (var b in exptected)
            {
                Assert.Equal(b, binData[i++]);
            }
        }

        [Fact]
        public void ShouldSendBinaryDataAsAnArraybuffer()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");

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
            Assert.True(Flag);
        }

        [Fact]
        public void BuffAck()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            Flag = false;

            var exptected = System.Text.Encoding.UTF8.GetBytes("asdfasdf");

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
            Assert.True(Flag);
            log.Info("Finish");
        }

        [Fact]
        public void DoubleCallTest()
        {
            ShouldSendBinaryDataAsAnArraybufferMixedWithJson();
            ShouldSendBinaryDataAsAnArraybufferMixedWithJson();
            ShouldSendBinaryDataAsAnArraybufferMixedWithJson();           
           
        }

        [Fact]
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
            Assert.True(Flag);
            log.Info("Finish");
        }

        [Fact]
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
            Assert.True(Flag);
        }


        [Fact]
        public void D10000CharsTest()
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
                log.Info("EVENT_CONNECT");
                socket.Emit("d10000chars");
            });

            socket.On("d10000chars",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE data="+data);
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = (string)events.Dequeue();
            Assert.Equal(10000, obj.Length);
        }


        [Fact]
        public void D100000CharsTest()
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
                log.Info("EVENT_CONNECT");
                socket.Emit("d100000chars");
            });

            socket.On("d100000chars",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE data=" + data);
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = (string)events.Dequeue();
            Assert.Equal(100000, obj.Length);
        }

        [Fact]
        public void Json10000CharsTest()
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
                log.Info("EVENT_CONNECT");
                socket.Emit("json10000chars");
            });

            socket.On("json10000chars",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE data=" + data);
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = (JObject)events.Dequeue();
            var str = (string)obj["data"];
            Assert.Equal(10000, str.Length);
        }

        [Fact]
        public void Json10000000CharsTest()
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
                log.Info("EVENT_CONNECT");
                socket.Emit("json10000000chars");
            });

            socket.On("json10000000chars",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE data=" + data);
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = (JObject)events.Dequeue();
            var str = (string)obj["data"];
            Assert.Equal(10000000, str.Length);
        }

        //Fails in .netstandard1.3 for SocketIoClientDotNet 1.0.4
        [Fact]
        public void Cookie()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();
            var uri = CreateUri();
            options.Cookies.Add("connect.sid","12345");
            socket = IO.Socket(uri, options);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                log.Info("EVENT_CONNECT");
                socket.Emit("get_cookie");                
            });

            socket.On("got_cookie",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE data=" + data);
                    events.Enqueue(data);
                    ManualResetEvent.Set();
                });

            ManualResetEvent.WaitOne();
            socket.Close();
            var cookie = (string)events.Dequeue();

            Assert.Equal("connect.sid=12345", cookie);
        }


        [Fact]
        public void MessageTestHebrew()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            ManualResetEvent = new ManualResetEvent(false);
            var events = new Queue<object>();

            var options = CreateOptions();


            options.Transports = ImmutableList.Create<string>(Polling.NAME);

            var uri = CreateUri();
            socket = IO.Socket(uri, options);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                log.Info("EVENT_CONNECT");
                socket.Emit("test", "csdataてすとבדיקה");
            });

            socket.On("hi",
                (data) =>
                {
                    log.Info("EVENT_MESSAGE");
                    events.Enqueue(data);
                    //socket.Emit("test", "2csdataてすとבדיקה");
                    ManualResetEvent.Set();
                });

            //socket.Open();
            ManualResetEvent.WaitOne();
            socket.Close();
            var obj = events.Dequeue();
            Assert.Equal("more data", obj);
        }


    }
}
