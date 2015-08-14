using System;
using Foundation;
using UIKit;

namespace SocketIoClientDotNet.Sample.XamariniOS
{
	partial class LoginViewController : UIViewController
	{
		public LoginViewController (IntPtr handle) 
			: base (handle)
		{
		}

		public override bool ShouldPerformSegue (string segueIdentifier, NSObject sender)
		{
			if (segueIdentifier == "loginSegue") {
				var username = usernameText.Text.Trim ();
				return !string.IsNullOrEmpty (username);
			}

			return base.ShouldPerformSegue (segueIdentifier, sender);
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "loginSegue") {
				var chat = segue.DestinationViewController as ChatViewController;
				chat.Username = usernameText.Text;
			}

			base.PrepareForSegue (segue, sender);
		}
	}
}
