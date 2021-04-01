using Bloggy.Helpers;
using Bloggy.Models;
using Bloggy.Services;
using Bloggy.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bloggy.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string email;
        private string password;
        public Command LoginCommand { get; }
        public Command RegisterCommand { get; }
        public Command PasswordForgotCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLogin, ValidateLogin);
            this.PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();

            RegisterCommand = new Command(OnRegister);
            PasswordForgotCommand = new Command(OnPasswordForgot);
        }

        private bool ValidateLogin()
        {
            return !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnRegister()
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new RegisterPage());
        }

        private void OnPasswordForgot()
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new ForgotPasswordPage());
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private async void OnLogin()
        {
            if (Constant.CheckConnectivity() == false)
            {
                await Application.Current.MainPage.DisplayAlert("Connection Error", "No internet connection. Please check and try again", "Cancel");
                return;
            }

            try
            {
                var Login = new LoginModel { Email = Email, Password = Password };
                Login.Email = new string(Login.Email.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());

                var result = await BloggyServices.LoginAsync(Login);

                if (result)
                {
                    Preferences.Set("Bloggy_Email", Login.Email);
                    Preferences.Set("Bloggy_Password", Login.Password);
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Login Error", "Something went wrong!", "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Login Error", "Something went wrong!", "Ok");
                Console.WriteLine(ex.Message);
            }


        }
    }
}
