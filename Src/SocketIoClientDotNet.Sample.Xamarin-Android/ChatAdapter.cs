using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace SocketIoClientDotNet.Sample.Xamarin_Android
{
    public class ChatAdapter : BaseAdapter<ChatAdapter.ChatItem>
    {
        private readonly List<ChatAdapter.ChatItem> chatItems;

        public ChatAdapter(List<ChatAdapter.ChatItem> chatItems)
        {
            this.chatItems = chatItems;
        }

        public override ChatAdapter.ChatItem this[int position]
        {
            get { return chatItems[position]; }
        }

        public override int Count
        {
            get { return chatItems.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = chatItems[position];
            var layout = item.Username == null 
                ? Android.Resource.Layout.SimpleListItem1 
                : Android.Resource.Layout.SimpleListItem2;

            var inflater = LayoutInflater.From(parent.Context);

            if (convertView == null || (int)convertView.Tag != layout)
            {
                convertView = inflater.Inflate(layout, parent, false);
                convertView.Tag = layout;
            }

            if (item.Username == null)
            {
                var text1 = convertView.FindViewById<TextView>(Android.Resource.Id.Text1);
                
                text1.Text = item.Message;
            }
            else
            {
                var text1 = convertView.FindViewById<TextView>(Android.Resource.Id.Text1);
                var text2 = convertView.FindViewById<TextView>(Android.Resource.Id.Text2);

                text1.Typeface = Typeface.DefaultBold;
                text1.Text = item.Username;
                text2.Text = item.Message;
            }

            return convertView;
        }

        public struct ChatItem
        {
            public ChatItem(string username, string message)
                : this()
            {
                Username = username;
                Message = message;
            }

            public string Username { get; set; }
            public string Message { get; set; }
        }
    }
}
