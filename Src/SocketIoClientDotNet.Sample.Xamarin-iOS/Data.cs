using Newtonsoft.Json.Linq;

namespace SocketIoClientDotNet.Sample.XamariniOS
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
