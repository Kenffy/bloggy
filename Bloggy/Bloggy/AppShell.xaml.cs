using System;
using System.Collections.Generic;
using Bloggy.ViewModels;
using Bloggy.Views;
using Xamarin.Forms;

namespace Bloggy
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(PostDetailPage), typeof(PostDetailPage));
            Routing.RegisterRoute(nameof(ImagePage), typeof(ImagePage));
        }

    }
}
