using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace SocketIoClientDotNet.Sample.Xamarin_Android
{
    public class LoginFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Login, container, false);

            var usernameText = view.FindViewById<EditText>(Resource.Id.usernameText);
            var loginButton = view.FindViewById<Button>(Resource.Id.loginButton);

            loginButton.Click += (sender, e) =>
            {
                var username = usernameText.Text.Trim();
                if (!string.IsNullOrEmpty(username))
                {
                    var activity = Activity as MainActivity;
                    if (activity != null)
                    {
                        activity.Login(username);
                    }
                }
            };

            return view;
        }
    }
}
