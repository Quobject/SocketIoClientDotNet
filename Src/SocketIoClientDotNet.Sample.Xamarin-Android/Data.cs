using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SocketIoClientDotNet.Sample.Xamarin_Android
{
    public class Data
    {
        public string username;
        public string message;
        public int numUsers;

        public static Data FromData(object data)
        {
            var json = data as JToken;
            if (json != null)
            {
                return json.ToObject<Data>();
            }
            return null;
        }
    }
}
