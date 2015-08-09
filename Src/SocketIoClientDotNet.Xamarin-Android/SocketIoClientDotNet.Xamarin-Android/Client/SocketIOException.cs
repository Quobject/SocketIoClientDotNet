using System;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class SocketIOException : Exception
    {
        public string Transport;
        public object code;

        public SocketIOException(string message)
            : base(message)
        {
        }


        public SocketIOException(Exception cause)
            : base("", cause)
        {
        }

        public SocketIOException(string message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
