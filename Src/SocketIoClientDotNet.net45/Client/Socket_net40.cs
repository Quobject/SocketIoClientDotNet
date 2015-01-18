
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Collections.Concurrent;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.SocketIoClientDotNet.Modules;
using Quobject.SocketIoClientDotNet.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class Socket : Emitter
    {
        public static readonly string EVENT_CONNECT = "connect";
        public static readonly string EVENT_DISCONNECT = "disconnect";
        public static readonly string EVENT_ERROR = "error";
        public static readonly string EVENT_MESSAGE = "message";
        public static readonly string EVENT_CONNECT_ERROR = Manager.EVENT_CONNECT_ERROR;
        public static readonly string EVENT_CONNECT_TIMEOUT = Manager.EVENT_CONNECT_TIMEOUT;
        public static readonly string EVENT_RECONNECT = Manager.EVENT_RECONNECT;
        public static readonly string EVENT_RECONNECT_ERROR = Manager.EVENT_RECONNECT_ERROR;
        public static readonly string EVENT_RECONNECT_FAILED = Manager.EVENT_RECONNECT_FAILED;
        public static readonly string EVENT_RECONNECT_ATTEMPT = Manager.EVENT_RECONNECT_ATTEMPT;
        public static readonly string EVENT_RECONNECTING = Manager.EVENT_RECONNECTING;

        private static readonly List<string> Events = new List<string>()
        {
            EVENT_CONNECT,
            EVENT_CONNECT_ERROR,
            EVENT_CONNECT_TIMEOUT,
            EVENT_DISCONNECT,
            EVENT_ERROR,
            EVENT_RECONNECT,
            EVENT_RECONNECT_ATTEMPT,
            EVENT_RECONNECT_FAILED,
            EVENT_RECONNECT_ERROR,
            EVENT_RECONNECTING
        };

        private bool Connected;
        //private bool Disconnected = true;
        private int Ids;
        private string Nsp;
        private Manager _io;
        private System.Collections.Concurrent.ConcurrentDictionary<int, IAck> Acks = new System.Collections.Concurrent.ConcurrentDictionary<int, IAck>();
        private ConcurrentQueue<On.IHandle> Subs;
        private ConcurrentQueue<List<object>> ReceiveBuffer = new ConcurrentQueue<List<object>>();
        private ConcurrentQueue<Parser.Packet> SendBuffer = new ConcurrentQueue<Parser.Packet>();

        public Socket(Manager io, string nsp)
        {
            _io = io;
            Nsp = nsp;
            this.SubEvents();
        }

        private void SubEvents()
        {
            Manager io = _io;
            Subs = new ConcurrentQueue<On.IHandle>();

            Subs.Enqueue(Client.On.Create(io, Manager.EVENT_OPEN, new ListenerImpl(OnOpen)));
            Subs.Enqueue(Client.On.Create(io, Manager.EVENT_PACKET, new ListenerImpl((data) => OnPacket((Packet)data))));
            Subs.Enqueue(Client.On.Create(io, Manager.EVENT_CLOSE, new ListenerImpl((data) => OnClose((string)data))));
        }


        public Socket Open()
        {
            var t = new Task(() =>
            {
                if (!Connected)
                {
                    _io.Open();
                    if (_io.ReadyState == Manager.ReadyStateEnum.OPEN)
                    {
                        OnOpen();
                    }
                }
            });
            t.Start();
            return this;
        }

        public Socket Connect()
        {
            return this.Open();
        }

        public Socket Send(params object[] args)
        {
            Emit(EVENT_MESSAGE, args);
            return this;
        }


        public override Emitter Emit(string eventString, params object[] args)
        {
            var log = LogManager.GetLogger(Global.CallerName());

            if (Events.Contains(eventString))
            {
                base.Emit(eventString, args);
                return this;
            }

            var _args = new List<object> {eventString};
            _args.AddRange(args);

            var jsonArgs = Parser.Packet.Args2JArray(_args);

            var parserType = HasBinaryData.HasBinary(jsonArgs) ? Parser.Parser.BINARY_EVENT : Parser.Parser.EVENT;
            var packet = new Packet(parserType, jsonArgs);

            var lastArg = _args[_args.Count - 1];
            if (lastArg is IAck)
            {
                log.Info(string.Format("emitting packet with ack id {0}", Ids));
                Acks.TryAdd(Ids, (IAck)lastArg);
                jsonArgs = Parser.Packet.Remove(jsonArgs, jsonArgs.Count - 1);
                packet.Data = jsonArgs;
                packet.Id = Ids++;
            }

            if (Connected)
            {
                Packet(packet);
            }
            else
            {
                SendBuffer.Enqueue(packet);
            }
            return this;
        }

 
        public Emitter Emit(string eventString, IAck ack, params object[] args)
        {
            var log = LogManager.GetLogger(Global.CallerName());

            var _args = new List<object> { eventString };
            if (args != null)
            {
                _args.AddRange(args);                
            }

            var jarray = new JArray(_args);
            var packet = new Packet(Parser.Parser.EVENT, jarray);

            log.Info(string.Format("emitting packet with ack id {0}", Ids));
            Acks.TryAdd(Ids, ack);
            packet.Id = Ids++;

            Packet(packet);
            return this;
        }

        public Emitter Emit(string eventString, Action ack, params object[] args)
        {
            return Emit(eventString, new AckImpl(ack), args);
        }
        public Emitter Emit(string eventString, Action<object> ack, params object[] args)
        {
            return Emit(eventString, new AckImpl(ack), args);
        }
        public Emitter Emit(string eventString, Action<object, object> ack, params object[] args)
        {
            return Emit(eventString, new AckImpl(ack), args);
        }
        public Emitter Emit(string eventString, Action<object, object, object> ack, params object[] args)
        {
            return Emit(eventString, new AckImpl(ack), args);
        }

        public void Packet(Packet packet)
        {
            packet.Nsp = Nsp;
            _io.Packet(packet);
        }

        private void OnOpen()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            if (Nsp != "/")
            {
                Packet(new Packet(Parser.Parser.CONNECT));
            }
        }

        private void OnClose(string reason)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("close ({0})", reason));
            Connected = false;
            Emit(EVENT_DISCONNECT, reason);
        }

        private void OnPacket(Packet packet)
        {
            if (Nsp != packet.Nsp)
            {
                return;
            }
            switch (packet.Type)
            {
                case Parser.Parser.CONNECT:
                    this.OnConnect();
                    break;

                case Parser.Parser.EVENT:
                    this.OnEvent(packet);
                    break;

                case Parser.Parser.BINARY_EVENT:
                    this.OnEvent(packet);
                    break;

                case Parser.Parser.ACK:
                    this.OnAck(packet);
                    break;

                case Parser.Parser.BINARY_ACK:
                    this.OnAck(packet);
                    break;

                case Parser.Parser.DISCONNECT:
                    this.OnDisconnect();
                    break;

                case Parser.Parser.ERROR:
                    this.Emit(EVENT_ERROR, packet.Data);
                    break;
            }
        }

 
        private void OnEvent(Packet packet)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            //var jarr =(string) ((JValue) packet.Data).Value;
            //var job = JToken.Parse(jarr);


            //var arr = job.ToArray();

            //var args = job.Select(token => token.Value<string>()).Cast<object>().ToList();
            var args = packet.GetDataAsList();



            log.Info(string.Format("emitting event {0}", args));
            if (packet.Id >= 0)
            {
                log.Info("attaching ack callback to event");
                args.Add(new AckImp(this, packet.Id));
            }

            if (Connected)
            {
                var eventString = (string) args[0];
                args.Remove(args[0]);
                base.Emit(eventString, args.ToArray());
            }
            else
            {
                ReceiveBuffer.Enqueue(args);
            }
        }  

        private class AckImp : IAck
        {
            private Socket socket;
            private int Id;
            private readonly bool[] sent = new[] {false};

            public AckImp(Socket socket, int id)
            {                
                this.socket = socket;
                this.Id = id;
            }

            public void Call(params object[] args)
            {
                if (sent[0])
                {
                    return;
                }
                sent[0] = true;
                var log = LogManager.GetLogger(Global.CallerName());
                var jsonArgs = Parser.Packet.Args2JArray(args);
                log.Info(string.Format("sending ack {0}", args.Length != 0 ? jsonArgs.ToString() : "null"));

                var parserType = HasBinaryData.HasBinary(args) ? Parser.Parser.BINARY_ACK : Parser.Parser.ACK;
                var packet = new Packet(parserType, jsonArgs);
                packet.Id = Id;
                socket.Packet(packet);
            }
        }

        private void OnAck(Parser.Packet packet)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("calling ack {0} with {1}", packet.Id, packet.Data));
            var fn = Acks[packet.Id];
            IAck outRef;
            Acks.TryRemove(packet.Id,out outRef );

            var args = packet.GetDataAsList();

            fn.Call(args.ToArray());
        }



        private void OnConnect()
        {
            Connected = true;
            //Disconnected = false;
            Emit(EVENT_CONNECT);
            EmitBuffered();
        }

        private void EmitBuffered()
        {
            while (ReceiveBuffer.Count > 0)
            {
                List<object> data;
                ReceiveBuffer.TryDequeue(out data);
                var eventString = (string) data[0];
                base.Emit(eventString, data.ToArray());
            }
            ReceiveBuffer.Clear();

           
            while (SendBuffer.Count > 0)
            {
                Packet packet;
                SendBuffer.TryDequeue(out packet);
                Packet(packet);
            }
            SendBuffer.Clear();
        }


        private void OnDisconnect()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("server disconnect ({0})", this.Nsp));
            Destroy();
            OnClose("io server disconnect");
        }

        private void Destroy()
        {
            foreach (var sub in Subs.GetEnumerator())
            {
                sub.Destroy();
            }
            Subs.Clear();

            _io.Destroy(this);
        }

        public Socket Close()
        {
            if (!Connected)
            {
                return this;
            }
            var log = LogManager.GetLogger(Global.CallerName());

            log.Info(string.Format("performing disconnect ({0})", Nsp));
            Packet(new Packet(Parser.Parser.DISCONNECT));
            Destroy();
            OnClose("io client disconnect");
            return this;
        }

        public Socket Disconnect()
        {
            return this.Close();
        }

        public Manager Io()
        {
            return _io;
        }

        private static IEnumerable<object> ToArray(JArray array)
        {
            int length = array.Count;
            var data = new object[length];
            for (int i = 0; i < length; i++)
            {
                object v;
                try
                {
                    v = array[i];
                }
                catch (Exception)
                {
                    v = null;
                }
                data[i] = v;
            }
            return data;
        }


    }
}
