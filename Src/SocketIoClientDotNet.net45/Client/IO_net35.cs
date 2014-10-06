

using System.Collections.Concurrent;
using Quobject.EngineIoClientDotNet.Modules;
using System;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class IO
    {
        private static readonly ConcurrentDictionary<string, Manager> Managers = new ConcurrentDictionary<string, Manager>();

        /// <summary>
        /// Protocol version
        /// </summary>
        public static int Protocol = Parser.Parser.protocol;

        private IO()
        {
            
        }

        public static Socket Socket(string uri)
        {
            return Socket(uri, null);
        }

        public static Socket Socket(string uri, Options opts)
        {
            return Socket(Url.Parse(uri), opts);
        }

        public static Socket Socket(Uri uri)
        {
            return Socket(uri, null);
        
        }
        public static Socket Socket(Uri uri, Options opts)
        {

            var log = LogManager.GetLogger(Global.CallerName());
            if (opts == null)
            {
                opts = new Options();
            }

            Manager io;

            if (opts.ForceNew || !opts.Multiplex)
            {
                log.Info(string.Format("ignoring socket cache for {0}", uri.ToString()));
                io = new Manager(uri, opts);
            }
            else
            {
                var id = Url.ExtractId(uri);
                if (!Managers.ContainsKey(id))
                {
                    log.Info( string.Format("new io instance for {0}", id));
                    Managers.TryAdd(id, new Manager(uri, opts));

                }
                io = Managers[id];
            }
            return io.Socket(uri.PathAndQuery);
        }


        public class Options : Client.Options
        {

            public bool ForceNew = true;            
            public bool Multiplex = true;
        }
    }
}
