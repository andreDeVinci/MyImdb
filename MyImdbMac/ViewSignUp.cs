﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using Foundation;
using AppKit;
using System.Linq;
using MyImdbLibrary;

namespace ImdbMacApp
{
	public partial class ViewSignUp : NSViewController
	{
        public DatabaseManager DbManager { get; set; }
        private User newUser;
        public ViewSignIn SignInView;

        public ViewSignUp (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();
            LoadCountryNames();
        }

        private void LoadCountryNames()
        {
            InputCountry.UsesDataSource = true;
            InputCountry.DataSource = new DataSource(DbManager.countriesNames);
            SetCurrentCountry();
        }

        private async void SetCurrentCountry()
        {
            string country_name = await InternetManager.GetCountryOfThisMachine(DbManager.countriesNamesAndCodes);

            if (country_name != null) InputCountry.SelectItem(DbManager.countriesNames.IndexOf(country_name));
        }

        private void AlertMessage(string message, string title)
        {
            var alert = new NSAlert()
            {
                AlertStyle = NSAlertStyle.Informational,
                InformativeText = message,
                MessageText = title,
            };
            alert.AddButton("Ok");
            alert.BeginSheet(View.Window);
        }

        partial void SignUpButton_Clicked(NSObject sender)
        {
            string FirstName    = InputFirstName.StringValue;
            string LastName     = InputLastName.StringValue;
            string Email        = InputEmail.StringValue;
            int country_id      = DbManager.countriesNamesAndCodes.FirstOrDefault(c => c.Name == InputCountry.StringValue).Id;
            string Password1    = InputPassword1.StringValue;
            string Password2    = InputPassword2.StringValue;

            if (FirstName.Any() && LastName.Any() && Email.Any() && (country_id > 0) && Password1.Any() && Password2.Any())
            {
                if (DatabaseManager.IsValidEmail(Email))
                {
                    if (DbManager.CheckIfEmailAddressIsFree(Email))
                    {
                        if (Password1.Length == 8)
                        {
                            if (Password1 == Password2)
                            {
                                newUser = new User(FirstName, LastName, country_id, Email, Password1, ConstAndParams.DefaultOmdbApiKey);
                                DbManager.AddUser(newUser);
                                SignInView.LoadEmailAddressAfterSignUpAndDisableButton(newUser.Email);
                                DismissViewController(this);
                            }
                            else
                            {
                                AlertMessage("Please re-enter the same password!", "Invalid Password!");
                                InputPassword2.StringValue = "";
                            }
                        }
                        else
                        {
                            AlertMessage("Please enter an 8 character password!", "Invalid Password!");
                            InputPassword1.StringValue = "";
                            InputPassword2.StringValue = "";
                        }
                    }
                    else AlertMessage("Email address is already used!", "Invalid Email!");
                }
                else AlertMessage("Please enter a valid email address.", "Invalid Email!");
            }
            else AlertMessage("Please fill all the fields.", "Unsufficient!");
        }

        partial void SignInButton_Clicked(NSObject sender) => DismissViewController(this);
    }
}
