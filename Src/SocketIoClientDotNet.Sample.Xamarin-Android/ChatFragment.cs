using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Quobject.SocketIoClientDotNet.Client;

namespace SocketIoClientDotNet.Sample.Xamarin_Android
{
    public class ChatFragment : Fragment
    {
        private const int TypingTimerDelay = 400; // ms

        private EditText entryText;
        private TextView typingText;
        private ListView chatWindow;

        private readonly string username;
        private readonly Socket socket;
        private readonly ChatAdapter adapter;

        private readonly List<ChatAdapter.ChatItem> chatItems = new List<ChatAdapter.ChatItem>();
        private List<string> typingItems = new List<string>();
        private bool connected = false;
        private bool typing = false;
        private DateTime lastTypingTime;
        private CancellationTokenSource cancellation = new CancellationTokenSource();

        public ChatFragment(string username, Socket socket, AlertDialog alert)
        {
            this.username = username;
            this.socket = socket;
            this.adapter = new ChatAdapter(chatItems);

            AttachSocketEvents(alert);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Chat, container, false);

            entryText = view.FindViewById<EditText>(Resource.Id.entryText);
            typingText = view.FindViewById<TextView>(Resource.Id.typingText);
            var sendButton = view.FindViewById<Button>(Resource.Id.sendButton);
            chatWindow = view.FindViewById<ListView>(Resource.Id.chatWindow);

            chatWindow.Adapter = adapter;

            entryText.Selected = true;

            entryText.TextChanged += (sender, e) =>
            {
                UpdateTyping();
            };
            sendButton.Click += (sender, e) =>
            {
                SendMessage();
                socket.Emit("stop typing");
                typing = false;
            };

            return view;
        }

        private void AttachSocketEvents(AlertDialog alert)
        {
            // Whenever the server emits "login", log the login message
            socket.On("login", data =>
            {
                if (alert != null)
                {
                    alert.Dismiss();
                    alert.Dispose();
                    alert = null;
                }

                var d = Data.FromData(data);
                connected = true;
                // Display the welcome message
                AddMessage("Welcome to Socket.IO Chat – ", true);
                AddParticipantsMessage(d.numUsers);
            });
            // Whenever the server emits "new message", update the chat body
            socket.On("new message", data =>
            {
                var d = Data.FromData(data);
                AddMessage(d.message, username: d.username);
            });
            // Whenever the server emits "user joined", log it in the chat body
            socket.On("user joined", data =>
            {
                var d = Data.FromData(data);
                AddMessage(d.username + " joined");
                AddParticipantsMessage(d.numUsers);
            });
            // Whenever the server emits "user left", log it in the chat body
            socket.On("user left", data =>
            {
                var d = Data.FromData(data);
                AddMessage(d.username + " left");
                AddParticipantsMessage(d.numUsers);
                UpdateChatTyping(d.username, true);
            });
            // Whenever the server emits "typing", show the typing message
            socket.On("typing", data =>
            {
                var d = Data.FromData(data);
                UpdateChatTyping(d.username, false);
            });
            // Whenever the server emits "stop typing", kill the typing message
            socket.On("stop typing", data =>
            {
                var d = Data.FromData(data);
                UpdateChatTyping(d.username, true);
            });
        }

        private void AddParticipantsMessage(int numUsers)
        {
            if (numUsers == 1)
                AddMessage("there's 1 participant");
            else
                AddMessage(string.Format("there are {0} participants", numUsers));
        }

        private void SendMessage()
        {
            var message = entryText.Text.Trim();
            // if there is a non-empty message and a socket connection
            if (!string.IsNullOrEmpty(message) && connected)
            {
                entryText.Text = string.Empty;
                AddMessage(message, username: username);
                // tell server to execute "new message" and send along one parameter
                socket.Emit("new message", message);
            }
        }

        private void UpdateTyping()
        {
            if (connected)
            {
                if (!typing)
                {
                    typing = true;
                    socket.Emit("typing");
                }
                lastTypingTime = DateTime.Now;

                // make sure we cancel any other typing
                cancellation.Cancel();
                cancellation = new CancellationTokenSource();
                // start the timer
                Task.Delay(TypingTimerDelay, cancellation.Token).ContinueWith(task =>
                {
                    var typingTimer = DateTime.Now;
                    var timeDiff = typingTimer - lastTypingTime;
                    if (timeDiff.TotalMilliseconds >= TypingTimerDelay && typing)
                    {
                        socket.Emit("stop typing");
                        typing = false;
                    }
                }, cancellation.Token);
            }
        }

        private void UpdateChatTyping(string username, bool remove)
        {
            var updated = false;
            if (remove)
            {
                if (typingItems.Contains(username))
                {
                    typingItems.Remove(username);
                    updated = true;
                }
            }
            else
            {
                if (!typingItems.Contains(username))
                {
                    typingItems.Add(username);
                    updated = true;
                }
            }

            if (updated && typingItems.Count > 0 && Activity != null)
            {
                Activity.RunOnUiThread(() =>
                {
                    typingText.Visibility = typingItems.Count > 0 ? ViewStates.Visible : ViewStates.Gone;

                    if (typingItems.Count == 1)
                        typingText.Text = typingItems[0] + " is typing...";
                    else
                        typingText.Text = string.Join(", ", typingItems) + " are typing...";
                });
            }
        }
        
        private void AddMessage(string message, bool prepend = false, string username = null)
        {
            if (Activity != null)
            {
                Activity.RunOnUiThread(() =>
                {
                    if (prepend)
                        chatItems.Insert(0, new ChatAdapter.ChatItem(username, message));
                    else
                        chatItems.Add(new ChatAdapter.ChatItem(username, message));

                    adapter.NotifyDataSetChanged();
                    chatWindow.SetSelection(chatWindow.Count);
                });
            }
        }
    }
}
