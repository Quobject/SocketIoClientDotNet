

using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Thread;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class Manager : Emitter
    {
        public enum ReadyStateEnum
        {
            OPENING,
            OPEN,
            CLOSED
        }

        public static readonly string EVENT_OPEN = "open";
        public static readonly string EVENT_CLOSE = "close";
        public static readonly string EVENT_PACKET = "packet";
        public static readonly string EVENT_ERROR = "error";
        public static readonly string EVENT_CONNECT_ERROR = "connect_error";
        public static readonly string EVENT_CONNECT_TIMEOUT = "connect_timeout";
        public static readonly string EVENT_RECONNECT = "reconnect";
        public static readonly string EVENT_RECONNECT_ERROR = "reconnect_error";
        public static readonly string EVENT_RECONNECT_FAILED = "reconnect_failed";
        public static readonly string EVENT_RECONNECT_ATTEMPT = "reconnect_attempt";
        public static readonly string EVENT_RECONNECTING = "reconnecting";


        /*package*/
        public ReadyStateEnum ReadyState = ReadyStateEnum.CLOSED;

        private bool _reconnection;
        private bool SkipReconnect;
        private bool Reconnecting;
        private bool Encoding;
        private bool OpenReconnect;
        private int _reconnectionAttempts;
        private long _reconnectionDelay;
        private long _reconnectionDelayMax;
        private long _timeout;
        private int Connected;
        private int Attempts;
        private Uri Uri;
        private List<Parser.Packet> PacketBuffer;
        private Queue<On.IHandle> Subs;
        private Quobject.EngineIoClientDotNet.Client.Socket.Options Opts;
        private bool AutoConnect;
        /*package*/

        public Quobject.EngineIoClientDotNet.Client.Socket EngineSocket;
        private Parser.Parser.Encoder Encoder;
        private Parser.Parser.Decoder Decoder;

        /**
         * This ConcurrentDictionary can be accessed from outside of EventThread.
         */
        private System.Collections.Concurrent.ConcurrentDictionary<string, Socket> Nsps;

        public Manager() : this(null, null)
        {

        }

        public Manager(Uri uri) : this(uri, null)
        {

        }

        public Manager(Options opts) : this(null, opts)
        {

        }

        public Manager(Uri uri, Options opts)
        {
            if (opts == null)
            {
                opts = new Options();
            }
            if (opts.Path == null)
            {
                opts.Path = "/socket.io";
            }
            this.Opts = opts;
            this.Nsps = new System.Collections.Concurrent.ConcurrentDictionary<string, Socket>();
            this.Subs = new Queue<On.IHandle>();
            this.Reconnection(opts.Reconnection);
            this.ReconnectionAttempts(opts.ReconnectionAttempts != 0 ? opts.ReconnectionAttempts : int.MaxValue);
            this.ReconnectionDelay(opts.ReconnectionDelay != 0 ? opts.ReconnectionDelay : 1000);
            this.ReconnectionDelayMax(opts.ReconnectionDelayMax != 0 ? opts.ReconnectionDelayMax : 5000);
            this.Timeout(opts.Timeout < 0 ? 20000 : opts.Timeout);
            this.ReadyState = ReadyStateEnum.CLOSED;
            this.Uri = uri;
            this.Connected = 0;
            this.Attempts = 0;
            this.Encoding = false;
            this.PacketBuffer = new List<Parser.Packet>();
            this.Encoder = new Parser.Parser.Encoder();
            this.Decoder = new Parser.Parser.Decoder();
            this.AutoConnect = opts.AutoConnect;
            if (AutoConnect)
            {
                Open();
            }
        }

        private void EmitAll(string eventString, params object[] args)
        {
            Emit(eventString, args);
            foreach (var socket in Nsps.Values)
            {
                socket.Emit(eventString, args);
            }
        }

        public bool Reconnection()
        {
            return _reconnection;
        }

        private Manager Reconnection(bool v)
        {
            _reconnection = v;
            return this;
        }


        public int ReconnectionAttempts()
        {
            return _reconnectionAttempts;
        }

        private Manager ReconnectionAttempts(int v)
        {
            _reconnectionAttempts = v;
            return this;
        }

        public long ReconnectionDelay()
        {
            return _reconnectionDelay;
        }

        private Manager ReconnectionDelay(long v)
        {
            _reconnectionDelay = v;
            return this;
        }

        public long ReconnectionDelayMax()
        {
            return _reconnectionDelayMax;
        }

        private Manager ReconnectionDelayMax(long v)
        {
            _reconnectionDelayMax = v;
            return this;
        }

        public long Timeout()
        {
            return _timeout;
        }

        private Manager Timeout(long v)
        {
            _timeout = v;
            return this;
        }

        private void MaybeReconnectOnOpen()
        {
            if (!this.OpenReconnect && !this.Reconnecting && this._reconnection)
            {
                this.OpenReconnect = true;
                this.Reconnect();
            }
        }

        public Manager Open()
        {
            return Open(null);
        }

        private Manager Open(IOpenCallback fn)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("readyState {0}", ReadyState));
            if (ReadyState == ReadyStateEnum.OPEN)
            {
                return this;
            }

            log.Info(string.Format("opening {0}", Uri));
            EngineSocket = new Engine(Uri, Opts);
            Quobject.EngineIoClientDotNet.Client.Socket socket = EngineSocket;

            ReadyState = ReadyStateEnum.OPENING;

            var openSub = SocketIoClientDotNet.Client.On.Create(socket, Engine.EVENT_OPEN, new ListenerImpl(() =>
            {
                OnOpen();
                if (fn != null)
                {
                    fn.Call(null);
                }
            }));

            var errorSub = Client.On.Create(socket, Engine.EVENT_ERROR, new ListenerImpl((data) =>
            {
                log.Info("connect_error");
                Cleanup();
                ReadyState = ReadyStateEnum.CLOSED;
                EmitAll(EVENT_CONNECT_ERROR, data);

                if (fn != null)
                {
                    var err = new SocketIOException("Connection error", data is Exception ? (Exception) data : null);
                    fn.Call(err);
                }
                MaybeReconnectOnOpen();
            }));

            if (_timeout >= 0)
            {
                var timeout = (int) _timeout;
                log.Info(string.Format("connection attempt will timeout after {0}", timeout));
                var timer = EasyTimer.SetTimeout(() =>
                {
                    var log2 = LogManager.GetLogger(Global.CallerName());
                    log2.Info("Manager Open start");

                    log2.Info(string.Format("connect attempt timed out after {0}", timeout));
                    openSub.Destroy();
                    socket.Close();
                    socket.Emit(Engine.EVENT_ERROR, new SocketIOException("timeout"));
                    EmitAll(EVENT_CONNECT_TIMEOUT, timeout);
                    log2.Info("Manager Open finish");

                }, timeout);
                lock (Subs)
                {
                    Subs.Enqueue(new On.ActionHandleImpl(timer.Stop));
                }

            }

            lock (Subs)
            {
                Subs.Enqueue(openSub);
                Subs.Enqueue(errorSub);
            }
            EngineSocket.Open();

            return this;
        }

        private void OnOpen()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("open");

            Cleanup();

            ReadyState = ReadyStateEnum.OPEN;
            Emit(EVENT_OPEN);

            var socket = EngineSocket;

            var sub = Client.On.Create(socket, Engine.EVENT_DATA, new ListenerImpl((data) =>
            {
                if (data is string)
                {
                    OnData((string)data);
                }
                else if (data is byte[])
                {
                    Ondata((byte[])data);
                }
            }));
            lock (Subs)
            {
                Subs.Enqueue(sub);
            }

            sub = Client.On.Create(this.Decoder, Parser.Parser.Decoder.EVENT_DECODED, new ListenerImpl((data) =>
            {
                OnDecoded((Parser.Packet)data);
            }));
            lock (Subs)
            {
                Subs.Enqueue(sub);
            }

            sub = Client.On.Create(socket, Engine.EVENT_ERROR, new ListenerImpl((data) =>
            {
                OnError((Exception) data);
            }));
            lock (Subs)
            {
                Subs.Enqueue(sub);
            }

            sub = Client.On.Create(socket, Engine.EVENT_CLOSE, new ListenerImpl((data) =>
            {
                OnClose((string) data);
            }));

            lock (Subs)
            {
                Subs.Enqueue(sub);
            }


        }

        private void OnData(string data)
        {
            this.Decoder.Add(data);
        }

        private void Ondata(byte[] data)
        {
            this.Decoder.Add(data);
        }

        private void OnDecoded(Parser.Packet packet)
        {
            this.Emit(EVENT_PACKET, packet);
        }

        private void OnError(Exception err)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Error("error", err);
            this.EmitAll(EVENT_ERROR, err);
        }

        public Socket Socket(string nsp)
        {
            if (Nsps.ContainsKey(nsp))
            {
                return Nsps[nsp];
            }

            var socket = new Socket(this,nsp);
            Nsps.TryAdd(nsp, socket);
            socket.On(Client.Socket.EVENT_CONNECT, new ListenerImpl(() =>
            {
                Connected++;
            }));
            return socket;
        }

        internal void Destroy(Socket socket)
        {
            --Connected;
            if (Connected == 0)
            {
                Close();
            }
        }


        internal void Packet(Parser.Packet packet)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("writing packet {0}", packet));

            if (!Encoding)
            {
                Encoding = true;
                Encoder.Encode(packet, new Parser.Parser.Encoder.CallbackImp((data) =>
                {
                    var encodedPackets = (object[]) data;
                    foreach (var packet1 in encodedPackets)
                    {
                        if (packet1 is string)
                        {
                            EngineSocket.Write((string) packet1);
                        }
                        else if (packet1 is byte[])
                        {
                            EngineSocket.Write((byte[]) packet1);
                        }
                    }
                    Encoding = false;
                    ProcessPacketQueue();
                }));
            }
            else
            {
                PacketBuffer.Add(packet);
            }

        }

        private void ProcessPacketQueue()
        {
            if (this.PacketBuffer.Count > 0 && !this.Encoding)
            {
                Parser.Packet pack = this.PacketBuffer[0];
                PacketBuffer.Remove(pack);
                this.Packet(pack);
            }
        }

        private void Cleanup()
        {
            lock (Subs)
            {
                foreach (var sub in Subs)
                {
                    sub.Destroy();
                }
                Subs.Clear();
            }
        }

        public void Close()
        {
            this.SkipReconnect = true;
            this.EngineSocket.Close();
        }


        private void OnClose(string reason)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("start");
            Cleanup();
            ReadyState = ReadyStateEnum.CLOSED;
            Emit(EVENT_CLOSE, reason);
            if (_reconnection && !SkipReconnect)
            {
                Reconnect();
            }
        }


        private void Reconnect()
        {
            var log = LogManager.GetLogger(Global.CallerName());

            if (Reconnecting)
            {
                return;
            }

            Attempts++;

            if (Attempts > _reconnectionAttempts)
            {
                log.Info("reconnect failed");
                EmitAll(EVENT_RECONNECT_FAILED);
                Reconnecting = false;
            }
            else
            {
                var delay = Attempts*ReconnectionDelay();
                delay = Math.Min(delay, ReconnectionDelayMax());
                log.Info(string.Format("will wait {0}ms before reconnect attempt", delay));

                Reconnecting = true;
                var timer = EasyTimer.SetTimeout(() =>
                {
                    var log2 = LogManager.GetLogger(Global.CallerName());
                    log2.Info("EasyTimer Reconnect start");
                    log2.Info(string.Format("attempting reconnect"));
                    EmitAll(EVENT_RECONNECT_ATTEMPT, Attempts);
                    EmitAll(EVENT_RECONNECTING, Attempts);
                    Open(new OpenCallbackImp((err) =>
                    {
                        if (err != null)
                        {
                            log.Error("reconnect attempt error", (Exception) err);
                            Reconnecting = false;
                            Reconnect();
                            EmitAll(EVENT_RECONNECT_ERROR, (Exception) err);
                        }
                        else
                        {
                            log.Info("reconnect success");
                            OnReconnect();
                        }
                    }));
                    log2.Info("EasyTimer Reconnect finish");
                }, (int)delay);

				lock (Subs){
					Subs.Enqueue(new On.ActionHandleImpl(timer.Stop));                
				}
            }
        }


        private void OnReconnect()
        {
            int attempts = this.Attempts;
            this.Attempts = 0;
            this.Reconnecting = false;
            this.EmitAll(EVENT_RECONNECT, attempts);
        }


        public interface IOpenCallback
        {

            void Call(Exception err);
        }

        public class OpenCallbackImp : IOpenCallback
        {
            private Action<object> Fn;

            public OpenCallbackImp(Action<object> fn)
            {
                Fn = fn;
            }

            public void Call(Exception err)
            {
                Fn(err);
            }
        }

    }



    public class Engine : Quobject.EngineIoClientDotNet.Client.Socket
    {
        public Engine(Uri uri, Options opts) : base(uri, opts)
        {

        }
    }



    public class Options : Quobject.EngineIoClientDotNet.Client.Socket.Options
    {

        public bool Reconnection = true;
        public int ReconnectionAttempts;
        public long ReconnectionDelay;
        public long ReconnectionDelayMax;
        public long Timeout = -1;
        public bool AutoConnect = true;
    }


}


