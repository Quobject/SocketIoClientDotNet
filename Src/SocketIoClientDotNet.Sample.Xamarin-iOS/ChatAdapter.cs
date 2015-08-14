using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace SocketIoClientDotNet.Sample.XamariniOS
{
	public class ChatAdapter : UITableViewDelegate, IUITableViewDataSource
	{
		private readonly List<ChatAdapter.ChatItem> chatItems;

		public ChatAdapter (List<ChatAdapter.ChatItem> chatItems)
		{
			this.chatItems = chatItems;
		}

		public nint RowsInSection (UITableView tableView, nint section)
		{
			return chatItems.Count;
		}

		public UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var item = chatItems [indexPath.Row];
			var cell = item.Username == null 
				? tableView.DequeueReusableCell ("singleLine")
				: tableView.DequeueReusableCell ("titledLine");

			if (item.Username == null) {
				cell.TextLabel.Text = item.Message;
			} else {
				cell.TextLabel.Text = item.Username;
				cell.DetailTextLabel.Text = item.Message;
			}

			return cell;
		}

		public struct ChatItem
		{
			public ChatItem (string username, string message)
				: this ()
			{
				Username = username;
				Message = message;
			}

			public string Username { get; set; }

			public string Message { get; set; }
		}
	}
}
