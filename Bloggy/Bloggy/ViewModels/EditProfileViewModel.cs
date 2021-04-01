﻿using Bloggy.Helpers;
using Bloggy.Models;
using Bloggy.Services;
using ImageToArray;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Bloggy.ViewModels
{
    public class EditProfileViewModel : BaseViewModel
    {
        private Image profileImage;
        private MemberDetail currentUser;
        public Command MediaCommand { get; }
        public Command UpdateProfileCommand { get; }
        public event EventHandler RefreshSettingsPage;
        public EditProfileViewModel(MemberDetail user)
        {
            currentUser = user;
            profileImage = new Image();
            MediaCommand = new Command(OnMedia);
            UpdateProfileCommand = new Command(UpdateProfile);
            InitProfileImage();
        }

        private void InitProfileImage()
        {
            if(CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.ProfileImage))
            {
                //var client = new System.Net.Http.HttpClient();
                //System.IO.Stream imagestream = await client.GetStreamAsync(CurrentUser.ProfileImage);
                //ProfileImage.Source = ImageSource.FromStream(() => imagestream);

                ProfileImage.Source = ImageSource.FromUri(new Uri(CurrentUser.ProfileImage));
            }
        }

        private async void UpdateProfile(object obj)
        {
            if (string.IsNullOrEmpty(CurrentUser.Name))
            {
                string msg = "Blog name is required.";
                await Application.Current.MainPage.DisplayAlert("Error updating a profile", msg, "OK");
                return;
            }

            if (Constant.CheckConnectivity() == false)
            {
                var msg = "No internet connection. Please check and try again";
                await Application.Current.MainPage.DisplayAlert("Connection Error", msg, "Cancel");
                return;
            }

            try
            {
                if (CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.Id))
                {
                    await BloggyServices.UpdateMemberAsync(CurrentUser);
                    // Messaging center update previous page
                    OnRefreshSettingsPage();
                    await Application.Current.MainPage.Navigation.PopAsync();
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

        private async void OnMedia(object obj)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await Application.Current.MainPage.DisplayAlert("ERROR", "Pick Photo is NOT supported", "OK");
                return;
            }

            //var file = await CrossMedia.Current.PickPhotoAsync();

            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium
            });

            if (file == null)
            {
                return;
            }

            ProfileImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });

            CurrentUser.ImageArray = FromFile.ToArray(file.GetStream());
        }

        public MemberDetail CurrentUser
        {
            get => currentUser;
            set => SetProperty(ref currentUser, value);
        }

        public Image ProfileImage
        {
            get => profileImage;
            set => SetProperty(ref profileImage, value);
        }

        private void OnRefreshSettingsPage()
        {
            if (RefreshSettingsPage != null)
            {
                RefreshSettingsPage(this, EventArgs.Empty);
            }
        }
    }
}
