using Bloggy.Helpers;
using Bloggy.Models;
using Bloggy.Services;
using Bloggy.Views;
using System;
using System.Linq;
using Xamarin.Forms;

namespace Bloggy.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string email;
        private string username;
        private string password;
        public Command LoginCommand { get; }
        public Command RegisterCommand { get; }
        public Command PasswordForgotCommand { get; }

        public RegisterViewModel()
        {
            LoginCommand = new Command(OnLogin);
            RegisterCommand = new Command(OnRegister, ValidateRegister);
            this.PropertyChanged +=
                (_, __) => RegisterCommand.ChangeCanExecute();
        }


        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private bool ValidateRegister()
        {
            return !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private async void OnRegister()
        {
            if (Constant.CheckConnectivity() == false)
            {
                await Application.Current.MainPage.DisplayAlert("Connection Error", "No internet connection. Please check and try again", "Cancel");
                return;
            }

            try
            {
                Email = new string(Email.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
                var Register = new RegisterModel { Email = Email, Username = Username, Password = Password };
                var result = await BloggyServices.RegisterAsync(Register);

                if (result)
                {
                    await Application.Current.MainPage.DisplayAlert("Hi", "Your account has been created", "Alright");
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Register Error", "Something went wrong!", "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Register Error", "Something went wrong!", "Ok");
                Console.WriteLine(ex.Message);
            }
        }

        private void OnLogin()
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new LoginPage());
        }
    }
}
