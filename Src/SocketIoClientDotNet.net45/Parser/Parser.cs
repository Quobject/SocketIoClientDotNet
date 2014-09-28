using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Text;


namespace Quobject.SocketIoClientDotNet.Parser
{
    public class Parser
    {
        public const int CONNECT = 0;
        public const int DISCONNECT = 1;
        public const int EVENT = 2;
        public const int ACK = 3;
        public const int ERROR = 4;
        public const int BINARY_EVENT = 5;
        public const int BINARY_ACK = 6;
        public const int protocol = 4;


        /// <summary>
        /// Packet types
        /// </summary>
        public static List<string> types = new List<string>()
        {
            "CONNECT",
            "DISCONNECT",
            "EVENT",
            "BINARY_EVENT",
            "ACK",
            "BINARY_ACK",
            "ERROR"
        };

        private Parser() { }

        private static Packet ErrorPacket = new Packet(ERROR, "parser error");

        public class Encoder
        {
            public Encoder() { }

            public interface ICallback
            {
                void Call(object[] data);
            }


            public void Encode(Packet obj, ICallback callback)
            {
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info(string.Format("encoding packet {0}", obj));

                if (BINARY_EVENT == obj.Type || BINARY_ACK == obj.Type)
                {
                    EncodeAsBinary(obj, callback);
                }
                else
                {
                    String encoding = EncodeAsString(obj);
                    callback.Call(new object[] { encoding });
                }
            }

            private string EncodeAsString(Packet obj)
            {
                var str = new StringBuilder();
                bool nsp = false;

                str.Append(obj.Type);

                if (BINARY_EVENT == obj.Type || BINARY_ACK == obj.Type)
                {
                    str.Append(obj.Attachments);
                    str.Append("-");
                }

                if (!string.IsNullOrEmpty(obj.Nsp) && !"/".Equals(obj.Nsp))
                {
                    nsp = true;
                    str.Append(obj.Nsp);
                }

                if (obj.Id >= 0)
                {
                    if (nsp)
                    {
                        str.Append(",");
                        nsp = false;
                    }
                    str.Append(obj.Id);
                }

                if (obj.Data != null)
                {
                    if (nsp) str.Append(",");
                    str.Append(obj.Data);
                }

                var log = LogManager.GetLogger(Global.CallerName());
                log.Info(string.Format("encoded {0} as {1}", obj, str));
                return str.ToString();
            }

            private void EncodeAsBinary(Packet obj, ICallback callback)
            {
                Binary.DeconstructedPacket deconstruction = Binary.DeconstructPacket(obj);
                String pack = EncodeAsString(deconstruction.Packet);
                var buffers = new List<object>();
                foreach (var item in deconstruction.Buffers)
                {
                    buffers.Add(item);
                }                    

                buffers.Insert(0, pack);
                callback.Call(buffers.ToArray());
            }

            public class CallbackImp : ICallback
            {
                private readonly Action<object[]> Fn;

                public CallbackImp(Action<object[]> fn)
                {
                    Fn = fn;
                }

                public void Call(object[] data)
                {
                    Fn(data);
                }
            }
        }


        public class Decoder : Emitter
        {
            public const string EVENT_DECODED = "decoded";

            /*package*/
            public BinaryReconstructor Reconstructor = null;

            public Decoder()
            {
                
            }

            public void Add(string obj)
            {
                Packet packet = decodeString(obj);
                if (packet.Type == BINARY_EVENT || packet.Type == BINARY_ACK)
                {
                    this.Reconstructor = new BinaryReconstructor(packet);

                    if (this.Reconstructor.reconPack.Attachments == 0)
                    {
                        this.Emit(EVENT_DECODED, packet);
                    }
                }
                else
                {
                    this.Emit(EVENT_DECODED, packet);
                }
            }
            

            public void Add(byte[] obj)
            {
                if (this.Reconstructor == null)
                {
                    throw new SocketIOException("got binary data when not reconstructing a packet");
                }
                else
                {
                    var packet = this.Reconstructor.TakeBinaryData(obj);
                    if (packet != null)
                    {
                        this.Reconstructor = null;
                        this.Emit(EVENT_DECODED, packet);
                    }
                }
            }

            private Packet decodeString(string str)
            {
                Packet p = new Packet();
                int i = 0;

                p.Type = int.Parse(str.Substring(0,1));
                if (p.Type < 0 || p.Type > types.Count - 1) return ErrorPacket;

                if (BINARY_EVENT == p.Type || BINARY_ACK == p.Type)
                {
                    StringBuilder attachments = new StringBuilder();
                    while (str.Substring(++i, 1) != "-")
                    {
                        attachments.Append(str.Substring(i, 1));
                    }
                    p.Attachments = int.Parse(attachments.ToString());
                }

                if (str.Length > i + 1 && "/" == str.Substring(i+1, 1))
                {
                    var nsp = new StringBuilder();
                    while (true)
                    {
                        ++i;
                        string c = str.Substring(i, 1);
                        if ("," == c)
                        {
                            break;
                        }
                        nsp.Append(c);
                        if (i + 1 == str.Length)
                        {
                            break;
                        }
                    }
                    p.Nsp = nsp.ToString();
                }
                else
                {
                    p.Nsp = "/";
                }

                var next = (i + 1) >= str.Length ? null : str.Substring(i + 1, 1);

                int unused;
                if (null != next && int.TryParse(next, out unused))
                {
                    var id = new StringBuilder();
                    while (true)
                    {
                        ++i;
                        var c = str.Substring(i, 1);

                        if (!int.TryParse(c, out unused))
                        {
                            --i;
                            break;
                        }
                        id.Append(c);
                        if (i + 1 >= str.Length)
                        {
                            break;
                        }
                    }
                    p.Id = int.Parse(id.ToString());
                }


                if (i++ < str.Length)
                {
                    try
                    {
                        var t = str.Substring(i);
                        p.Data = new JValue(t);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // do nothing
                    }
                    catch (Exception)
                    {
                        return ErrorPacket;
                    }                    
                }
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info(string.Format("decoded {0} as {1}", str, p));
                return p;
            }

            public void Destroy()
            {
                if (Reconstructor != null)
                {
                    Reconstructor.FinishReconstruction();
                }
            }


        }

        /*package*/
        public class BinaryReconstructor
        {

            public Packet reconPack;

            /*package*/
            public List<byte[]> Buffers;

            public BinaryReconstructor(Packet packet)
            {
                this.reconPack = packet;
                this.Buffers = new List<byte[]>();
            }

            public Packet TakeBinaryData(byte[] binData)
            {
                this.Buffers.Add(binData);
                if (this.Buffers.Count == this.reconPack.Attachments)
                {
                    Packet packet = Binary.ReconstructPacket(this.reconPack,
                            this.Buffers.ToArray());
                    this.FinishReconstruction();
                    return packet;
                }
                return null;
            }

            public void FinishReconstruction()
            {
                this.reconPack = null;
                this.Buffers = new List<byte[]>();
            }
        }

    }
}
