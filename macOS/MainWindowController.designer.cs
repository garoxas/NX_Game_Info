// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace NX_Game_Info
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSButton cancel { get; set; }

		[Outlet]
		AppKit.NSMenu contextMenu { get; set; }

		[Outlet]
		AppKit.NSTextField message { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator progress { get; set; }

		[Outlet]
		AppKit.NSWindow sheet { get; set; }

		[Outlet]
		AppKit.NSTableView tableView { get; set; }

		[Outlet]
		AppKit.NSTextField title { get; set; }

		[Action ("cancelProgress:")]
		partial void cancelProgress (Foundation.NSObject sender);

		[Action ("debugLog:")]
		partial void debugLog (Foundation.NSObject sender);

		[Action ("export:")]
		partial void export (Foundation.NSObject sender);

		[Action ("showInFinder:")]
		partial void showInFinder (Foundation.NSObject sender);

		[Action ("updateTitleKeys:")]
		partial void updateTitleKeys (Foundation.NSObject sender);

		[Action ("updateVersionList:")]
		partial void updateVersionList (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (cancel != null) {
				cancel.Dispose ();
				cancel = null;
			}

			if (contextMenu != null) {
				contextMenu.Dispose ();
				contextMenu = null;
			}

			if (message != null) {
				message.Dispose ();
				message = null;
			}

			if (progress != null) {
				progress.Dispose ();
				progress = null;
			}

			if (sheet != null) {
				sheet.Dispose ();
				sheet = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (title != null) {
				title.Dispose ();
				title = null;
			}
		}
	}
}
