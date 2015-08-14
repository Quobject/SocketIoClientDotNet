using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

using Quobject.SocketIoClientDotNet.Client;

namespace SocketIoClientDotNet.Sample.XamariniOS
{
	partial class ChatViewController : UIViewController
	{
		private const int TypingTimerDelay = 400; // ms

		private Socket socket;
		private readonly ChatAdapter adapter;
		private UITableView chatWindow;

		private readonly List<ChatAdapter.ChatItem> chatItems = new List<ChatAdapter.ChatItem> ();
		private List<string> typingItems = new List<string> ();
		private bool connected = false;
		private bool typing = false;

		public ChatViewController (IntPtr handle)
			: base (handle)
		{
			adapter = new ChatAdapter (chatItems);
		}

		public string Username { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (socket != null) {
				socket.Close ();
			}

			socket = IO.Socket ("http://chat.socket.io/");
			socket.Connect ();

			var alert = new UIAlertView ("Log in", "Logging in...", null, null, null);
			alert.Show ();

			AttachSocketEvents (alert);

			// Tell the server your username (login)
			socket.Emit ("add user", Username);

			entryText.Selected = true;
			entryText.Started += (sender, e) => {
				if (connected) {
					if (!typing) {
						typing = true;
						socket.Emit ("typing");
					}
				}
			};
			entryText.Ended += (sender, e) => {
				if (connected) {
					if (typing) {
						socket.Emit ("stop typing");
						typing = false;
					}
				}
			};
			sendButton.TouchUpInside += (sender, e) => {
				SendMessage ();
				socket.Emit ("stop typing");
				typing = false;
			};

			// scroll up when the keyboard appears
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, notification => {
				var info = notification.UserInfo;
				var kbFrame = (NSValue)info [UIKeyboard.FrameEndUserInfoKey];
				var kbDuration = (NSNumber)info [UIKeyboard.AnimationDurationUserInfoKey];
				var animationDuration = kbDuration.DoubleValue;
				var keyboardFrame = kbFrame.CGRectValue;

				nfloat height = keyboardFrame.Size.Height + 8;

				entryTextBottom.Constant = height;
				sendButtonBottom.Constant = height;

				UIView.Animate (animationDuration, () => View.LayoutIfNeeded ());
			});
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, notification => {
				var info = notification.UserInfo;
				var kbDuration = (NSNumber)info [UIKeyboard.AnimationDurationUserInfoKey];
				var animationDuration = kbDuration.DoubleValue;

				entryTextBottom.Constant = 8;
				sendButtonBottom.Constant = 8;

				UIView.Animate (animationDuration, () => View.LayoutIfNeeded ());
			});
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, Foundation.NSObject sender)
		{
			if (segue.Identifier == "chatWindow") {
				var tvc = segue.DestinationViewController as UITableViewController;
				chatWindow = tvc.TableView;
				chatWindow.Delegate = adapter;
				chatWindow.DataSource = adapter;
			}

			base.PrepareForSegue (segue, sender);
		}

		private void AttachSocketEvents (UIAlertView alert)
		{
			// Whenever the server emits "login", log the login message
			socket.On ("login", data => {
				if (alert != null) {
					InvokeOnMainThread (() => alert.DismissWithClickedButtonIndex (0, true));
					alert = null;
				}

				var d = Data.FromData (data);
				connected = true;
				// Display the welcome message
				AddMessage ("Welcome to Socket.IO Chat – ", true);
				AddParticipantsMessage (d.numUsers);
			});
			// Whenever the server emits "new message", update the chat body
			socket.On ("new message", data => {
				var d = Data.FromData (data);
				AddMessage (d.message, username: d.username);
			});
			// Whenever the server emits "user joined", log it in the chat body
			socket.On ("user joined", data => {
				var d = Data.FromData (data);
				AddMessage (d.username + " joined");
				AddParticipantsMessage (d.numUsers);
			});
			// Whenever the server emits "user left", log it in the chat body
			socket.On ("user left", data => {
				var d = Data.FromData (data);
				AddMessage (d.username + " left");
				AddParticipantsMessage (d.numUsers);
				UpdateChatTyping (d.username, true);
			});
			// Whenever the server emits "typing", show the typing message
			socket.On ("typing", data => {
				var d = Data.FromData (data);
				UpdateChatTyping (d.username, false);
			});
			// Whenever the server emits "stop typing", kill the typing message
			socket.On ("stop typing", data => {
				var d = Data.FromData (data);
				UpdateChatTyping (d.username, true);
			});
		}

		private void AddParticipantsMessage (int numUsers)
		{
			if (numUsers == 1)
				AddMessage ("there's 1 participant");
			else
				AddMessage (string.Format ("there are {0} participants", numUsers));
		}

		private void SendMessage ()
		{
			var message = entryText.Text.Trim ();
			// if there is a non-empty message and a socket connection
			if (!string.IsNullOrEmpty (message) && connected) {
				entryText.Text = string.Empty;
				AddMessage (message, username: Username);
				// tell server to execute "new message" and send along one parameter
				socket.Emit ("new message", message);
			}
		}

		private void UpdateChatTyping (string username, bool remove)
		{
			var updated = false;
			if (remove) {
				if (typingItems.Contains (username)) {
					typingItems.Remove (username);
					updated = true;
				}
			} else {
				if (!typingItems.Contains (username)) {
					typingItems.Add (username);
					updated = true;
				}
			}

			if (updated && typingItems.Count > 0) {
				InvokeOnMainThread (() => {
					typingText.Hidden = typingItems.Count == 0;

					if (typingItems.Count == 1)
						typingText.Text = typingItems [0] + " is typing...";
					else
						typingText.Text = string.Join (", ", typingItems) + " are typing...";
				});
			}
		}

		private void AddMessage (string message, bool prepend = false, string username = null)
		{
			InvokeOnMainThread (() => {
				if (prepend)
					chatItems.Insert (0, new ChatAdapter.ChatItem (username, message));
				else
					chatItems.Add (new ChatAdapter.ChatItem (username, message));

				chatWindow.ReloadData ();
				var lastIndex = NSIndexPath.FromRowSection (chatItems.Count - 1, 0);
				chatWindow.ScrollToRow (lastIndex, UITableViewScrollPosition.Bottom, true);
			});
		}
	}
}
