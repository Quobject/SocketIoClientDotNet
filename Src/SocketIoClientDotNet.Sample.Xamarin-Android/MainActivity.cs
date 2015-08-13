using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Quobject.SocketIoClientDotNet.Client;

using AppCompatActivity = Android.Support.V7.App.AppCompatActivity;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace SocketIoClientDotNet.Sample.Xamarin_Android
{
    [Activity(MainLauncher = true, Theme = "@style/MyTheme", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : AppCompatActivity
    {
        private Socket socket;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = Resources.GetString(Resource.String.ApplicationName);

            var login = new LoginFragment();
            FragmentManager.BeginTransaction()
                           .Add(Resource.Id.frameLayout, login)
                           .Commit();
        }

        public void Login(string username)
        {
            if (socket != null)
            {
                socket.Close();
            }

            socket = IO.Socket("http://chat.socket.io/");
            socket.Connect();

            var alert = new AlertDialog.Builder(this).SetMessage("Logging in...").Show();
            var chat = new ChatFragment(username, socket, alert);
            FragmentManager.BeginTransaction()
                           .Replace(Resource.Id.frameLayout, chat)
                           .Commit();

            // Tell the server your username (login)
            socket.Emit("add user", username);
        }
    }
}
