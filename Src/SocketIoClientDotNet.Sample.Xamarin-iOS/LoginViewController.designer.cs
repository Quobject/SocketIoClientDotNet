// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace SocketIoClientDotNet.Sample.XamariniOS
{
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton loginButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField usernameText { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (loginButton != null) {
				loginButton.Dispose ();
				loginButton = null;
			}
			if (usernameText != null) {
				usernameText.Dispose ();
				usernameText = null;
			}
		}
	}
}
