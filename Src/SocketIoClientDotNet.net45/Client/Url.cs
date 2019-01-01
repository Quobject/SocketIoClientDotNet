using System;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class Url
    {
        private Url() { }



        public static Uri Parse(string uri)
        {
            if (uri.StartsWith("//"))
            {
                uri = "http:" + uri;
            }

            var result = new Uri(uri);
            return result;

        }

        public static string ExtractId(string url)
        {
            return ExtractId(new Uri(url));
        }

        public static string ExtractId(Uri uri)
        {  
            var protocol = uri.Scheme;
            int port = uri.Port;
            if (port == -1)
            {
                if (uri.Scheme.StartsWith("https"))
                {
                    port = 443;
                }else if (uri.Scheme.StartsWith("http"))
                {
                    port = 80;
                }
            }

            return string.Format("{0}://{1}:{2}", protocol, uri.Host , port);
        }
    }
}
