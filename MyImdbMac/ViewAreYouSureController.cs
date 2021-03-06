﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using Foundation;
using AppKit;
using MyImdbLibrary;

namespace ImdbMacApp
{
	public partial class ViewAreYouSureController : NSViewController
	{
        public int MainUser_id { get; set; }
        public int Id_movieToDelete { get; set; }
        public DatabaseManager DbManager;
        public ViewController MainView;

        public ViewAreYouSureController (IntPtr handle) : base (handle) { }

        partial void YesButton_Clicked(NSObject sender)
        {
            DbManager.RemoveMovieForUser(Id_movieToDelete, MainUser_id);
            MainView.ClearAndStartDisplay();
            DismissViewController(this);
        }

        partial void NoButton_Clicked(NSObject sender)
        {
            DismissViewController(this);
        }
    }
}
