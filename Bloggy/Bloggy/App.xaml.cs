﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bloggy.Views;
using Xamarin.Essentials;
using MonkeyCache.FileStore;

namespace Bloggy
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            Barrel.ApplicationId = AppInfo.PackageName;
            if (string.IsNullOrEmpty(Preferences.Get("BloggyToken", string.Empty)))
            {
                MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                MainPage = new AppShell();
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
