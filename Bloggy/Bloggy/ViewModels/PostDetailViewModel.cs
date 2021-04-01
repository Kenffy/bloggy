using Bloggy.Helpers;
using Bloggy.Models;
using Bloggy.Services;
using Bloggy.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Bloggy.ViewModels
{
    [QueryProperty(nameof(PostId), nameof(PostId))]
    public class PostDetailViewModel : BaseViewModel
    {
        private string postId;
        private CommentDetail comment;
        private PostDetail currentPost;
        private ObservableCollection<CommentDetail> comments;


        public Command SaveCommand { get; }
        public Command LikePostCommand { get; }
        public Command DisplayImageCommand { get; }

        public PostDetailViewModel()
        {
            currentPost = new PostDetail();
            comment = new CommentDetail();
            SaveCommand = new Command(SaveComment);
            LikePostCommand = new Command(OnLikePost);
            DisplayImageCommand = new Command(DisplayImage);

            comments = new ObservableCollection<CommentDetail>();
        }

        public string PostId
        {
            get => postId;
            set
            {
                postId = value;
                LoadCurrentPost(value);
            }
        }

        public CommentDetail Comment
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

        public PostDetail CurrentPost
        {
            get => currentPost;
            set => SetProperty(ref currentPost, value);
        }

        public ObservableCollection<CommentDetail> Comments
        {
            get => comments;
            set => SetProperty(ref comments, value);
        }

        private async void LoadCurrentPost(string postid)
        {
            if (Constant.CheckConnectivity() == false)
            {
                var msg = "No internet connection. Please check and try again";
                await Application.Current.MainPage.DisplayAlert("Connection Error", msg, "Cancel");
                return;
            }

            try
            {
                //var post = JsonConvert.DeserializeObject<PostDetail>(jsonStr);
                Guid Id = new Guid(postid);
                var post = await BloggyServices.GetPostByIdAsync(Id);
                CurrentPost = post;

                if (CurrentPost != null)
                {
                    Title = CurrentPost.ShortTitle;
                    var res = await BloggyServices.GetCommentsByPostIdAsync(CurrentPost.Id);
                    foreach (var comment in res)
                    {
                        Comments.Add(comment);
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

        private async void DisplayImage(object obj)
        {
            if (string.IsNullOrEmpty(CurrentPost.PostImage))
                return;

            var imagepage = new ImagePage { BindingContext = new ImageViewModel(CurrentPost.PostImage) };
            await Shell.Current.Navigation.PushModalAsync(imagepage);
        }

        private async void SaveComment(object obj)
        {
            if (string.IsNullOrEmpty(Comment.Body))
                return;

            if (Constant.CheckConnectivity() == false)
            {
                var msg = "No internet connection. Please check and try again";
                await Application.Current.MainPage.DisplayAlert("Connection Error", msg, "Cancel");
                return;
            }

            Comment.Id = Guid.NewGuid();

            if (CurrentPost != null && Comment != null && !string.IsNullOrEmpty(Comment.Body))
            {
                try
                {
                    Comment.PostId = CurrentPost.Id;
                    await BloggyServices.CreatePostCommentAsync(Comment);
                    MessagingCenter.Send(this, "UpsertPostStatus", true);
                    Comments = await BloggyServices.GetCommentsByPostIdAsync(CurrentPost.Id);
                    Comment = new CommentDetail();
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send(this, "UpsertPostStatus", false);
                    var msg = "An error occured, Something went wrong";
                    Console.WriteLine(ex.Message);
                    await Application.Current.MainPage.DisplayAlert("Error", msg, "Cancel");
                    return;
                }
            }
        }

        private async void OnLikePost(object obj)
        {
            if (Constant.CheckConnectivity() == false)
            {
                var msg = "No internet connection. Please check and try again";
                await Application.Current.MainPage.DisplayAlert("Connection Error", msg, "Cancel");
                return;
            }

            try
            {
                PostLike like = new PostLike
                {
                    Id = Guid.NewGuid(),
                    PostId = CurrentPost.Id
                };
                await BloggyServices.LikePostAsync(like);
                MessagingCenter.Send(this, "UpsertPostStatus", true);
                var post = await BloggyServices.GetPostByIdAsync(like.PostId);
                CurrentPost = post;
            }
            catch (Exception ex)
            {
                MessagingCenter.Send(this, "UpsertPostStatus", false);
                var msg = "An error occured, Something went wrong";
                Console.WriteLine(ex.Message);
                await Application.Current.MainPage.DisplayAlert("Error", msg, "Cancel");
                return;
            }
        }
    }

    //ShellNavigationState state = Shell.Current.CurrentState;
    //await Shell.Current.GoToAsync($"{state.Location}/{destinationRoute}?name={animalName}");
    //await Shell.Current.Navigation.PushModalAsync(new ImagePage { BindingContext = new ImageViewModel(CurrentPost.PostImage) });
    //await Shell.Current.GoToAsync($"{state.Location}/{nameof(ImagePage)}?{nameof(ImageViewModel.ImageUrl)}={CurrentPost.PostImage}");
}
