using Bloggy.Helpers;
using Bloggy.Models;
using Bloggy.Services;
using Bloggy.Views;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bloggy.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private string number = "+49 17671681034";
        private string email = "kenffy10@gmail.com";
        private string version = "Version 1.0.1";

        private MemberDetail currentUser;
        public Command OnPostsCommand { get; }
        public Command OnMemberCommand { get; }
        public Command DisplayImageCommand { get; }
        public Command PrivacyPolicyCommand { get; }
        public Command TermsOfUseCommand { get; }
        public Command WebsiteCommand { get; }
        public Command RateAppCommand { get; }
        public Command OnMessageCommand { get; }
        public Command OnSMSCommand { get; }
        public Command OnCallCommand { get; }

        public AboutViewModel()
        {
            Title = "About";
            currentUser = new MemberDetail();
            OnPostsCommand = new Command(OnPosts);
            OnMemberCommand = new Command(OnMembers);
            DisplayImageCommand = new Command(OnDisplayImage);
            PrivacyPolicyCommand = new Command(OnPrivacyPolicy);
            TermsOfUseCommand = new Command(OnTermsOfUse);
            WebsiteCommand = new Command(OnWebsite);
            RateAppCommand = new Command(OnRateApp);
            OnMessageCommand = new Command(OnMessage);
            OnSMSCommand = new Command(OnSMS);
            OnCallCommand = new Command(OnCall);
            LoadUserInfos();
        }

        private async void OnRateApp(object obj)
        {
            var msg = "Currently not available";
            await Application.Current.MainPage.DisplayAlert("Rate the Application", msg, "Cancel");
            return;
        }

        private async void OnWebsite(object obj)
        {
            await Browser.OpenAsync("https://aka.ms/xamarin-quickstart");
            //var msg = "Currently not available!!!";
            //await Application.Current.MainPage.DisplayAlert("Rate the Application", msg, "Cancel");
            //return;
        }

        private async void OnTermsOfUse(object obj)
        {
            var msg = "Currently not available!!!";
            await Application.Current.MainPage.DisplayAlert("Terms of use", msg, "Cancel");
            return;
        }

        private async void OnPrivacyPolicy(object obj)
        {
            var msg = "Currently not available!!!";
            await Application.Current.MainPage.DisplayAlert("Privacy Policy", msg, "Cancel");
            return;
        }

        public MemberDetail CurrentUser
        {
            get => currentUser;
            set => SetProperty(ref currentUser, value);
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string Number
        {
            get => number;
            set => SetProperty(ref number, value);
        }

        public string Version
        {
            get => version;
            set => SetProperty(ref version, value);
        }

        private async void OnPosts(object obj)
        {
            var detail = new BloggyDetailPage();
            await Shell.Current.Navigation.PushAsync(detail); //new BloggyDetailPage(CurrentUser, 0)
        }

        private async void OnMembers(object obj)
        {
            var detail = new BloggyDetailPage();
            await Shell.Current.Navigation.PushAsync(detail); //new BloggyDetailPage(CurrentUser, 1)
        }

        private async void OnDisplayImage(object obj)
        {
            if (string.IsNullOrEmpty(CurrentUser.ProfileImage))
                return;

            var imagepage = new ImagePage { BindingContext = new ImageViewModel(CurrentUser.ProfileImage) };
            await Shell.Current.Navigation.PushModalAsync(imagepage);
        }

        private void OnMessage(object obj)
        {
            var message = new EmailMessage("", "", Email);
            Xamarin.Essentials.Email.ComposeAsync(message);
        }

        private void OnSMS(object obj)
        {
            var message = new SmsMessage("", Number);
            Sms.ComposeAsync(message);
        }

        private void OnCall(object obj)
        {
            PhoneDialer.Open(Number);
        }

        private async void LoadUserInfos()
        {
            if (Constant.CheckConnectivity() == false)
            {
                var msg = "No internet connection. Please check and try again";
                await Application.Current.MainPage.DisplayAlert("Connection Error", msg, "Cancel");
                return;
            }

            try
            {
                CurrentUser = await BloggyServices.GetBloggyInfoAsync();

                if (CurrentUser != null)
                {

                    Title = "About " + CurrentUser.Name;
                    Version = CurrentUser.Name + ": Version 1.0.0";

                    if (!string.IsNullOrEmpty(CurrentUser.PhoneNumber))
                    {
                        Number = CurrentUser.PhoneNumber;
                    }

                    if (!string.IsNullOrEmpty(CurrentUser.Email))
                    {
                        Email = CurrentUser.Email;
                    }
                }

            }
            catch (Exception ex)
            {
                var msg = "An error occured, Something went wrong";
                Console.WriteLine(ex.Message);
                await Application.Current.MainPage.DisplayAlert("Error", msg, "Cancel");
                return;
            }
        }
    }
}