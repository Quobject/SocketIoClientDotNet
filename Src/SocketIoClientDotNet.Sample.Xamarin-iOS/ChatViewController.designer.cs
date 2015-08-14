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
	[Register ("ChatViewController")]
	partial class ChatViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField entryText { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		NSLayoutConstraint entryTextBottom { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton sendButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		NSLayoutConstraint sendButtonBottom { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel typingText { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (entryText != null) {
				entryText.Dispose ();
				entryText = null;
			}
			if (entryTextBottom != null) {
				entryTextBottom.Dispose ();
				entryTextBottom = null;
			}
			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}
			if (sendButtonBottom != null) {
				sendButtonBottom.Dispose ();
				sendButtonBottom = null;
			}
			if (typingText != null) {
				typingText.Dispose ();
				typingText = null;
			}
		}
	}
}
