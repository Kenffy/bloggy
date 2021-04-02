using Bloggy.Helpers;
using Bloggy.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Bloggy.Services
{
    public static class BloggyServices
    {
        public static async Task<bool> RegisterAsync(RegisterModel model)
        {

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password);
            await authProvider.UpdateProfileAsync(auth.FirebaseToken, model.Username, "");

            var client = new FirebaseClient(BloggyConstant.ConnString,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(auth.FirebaseToken) });

            Member member = new Member
            {
                Id = auth.User.LocalId,
                Name = model.Username,
                Description = "Hello Bloggy",
                Avatar = model.Username.ToUpper().FirstOrDefault().ToString(),
                AvatarColor = BloggyColor.GetRandomHexColor(),
                Email = model.Email,
                ProfileImage = "",
                Role = "User",
                PhoneNumber = ""
            };

            await client.Child("Members").PostAsync(member);

            var toUpdateUser = (await client.Child("Members")
                    .OnceAsync<Member>()).FirstOrDefault
                    (a => a.Object.Role == Constant.AdminRole);

            var user = toUpdateUser.Object;
            var users = await GetAllMembersAsync();
            user.NumMembers = users.Count();
            await client.Child("Members").Child(toUpdateUser.Key).PutAsync(user);

            return (auth.FirebaseToken != null);
        }
        public static async Task<bool> LoginAsync(LoginModel model)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            var auth = await authProvider.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
            var content = await auth.GetFreshAuthAsync();

            var serializedcontent = JsonConvert.SerializeObject(content);
            Preferences.Set("BloggyToken", serializedcontent);

            return (auth.FirebaseToken != null);
        }
        public static async Task ChangePasswordAsync(string email)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            await authProvider.SendPasswordResetEmailAsync(email);
        }
        public static async Task<List<MemberDetail>> GetAllMembersAsync()
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var members = (await client
                  .Child("Members")
                  .OnceAsync<Member>()).Select(item => new MemberDetail
                  {
                      Id = item.Object.Id,
                      Role = item.Object.Role,
                      Name = item.Object.Name,
                      Avatar = item.Object.Avatar,
                      AvatarColor = item.Object.AvatarColor,
                      Email = item.Object.Email,
                      Description = item.Object.Description,
                      ProfileImage = item.Object.ProfileImage,
                      PhoneNumber = item.Object.PhoneNumber,
                      NumPosts = item.Object.NumPosts,
                      NumMembers = item.Object.NumMembers
                  }).Where(m => m.Role != Constant.AdminRole).ToList();

            return members;
        }
        public static async Task<MemberDetail> GetBloggyInfoAsync()
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var bloggyInfos = (await client.Child("Members").OnceAsync<Member>())
                .Select(member => new MemberDetail
                {
                    Id = member.Object.Id,
                    Role = member.Object.Role,
                    Name = member.Object.Name,
                    Avatar = member.Object.Avatar,
                    AvatarColor = member.Object.AvatarColor,
                    Email = member.Object.Email,
                    Description = member.Object.Description,
                    ProfileImage = member.Object.ProfileImage,
                    PhoneNumber = member.Object.PhoneNumber,
                    NumPosts = member.Object.NumPosts,
                    NumMembers = member.Object.NumMembers
                }).Where(a => a.Role == Constant.AdminRole).FirstOrDefault();

            return bloggyInfos;
        }
        public static async Task<MemberDetail> GetAuthMemberAsync()
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            var user = await authProvider.GetUserAsync(token.FirebaseToken);
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var authUser = (await client.Child("Members").OnceAsync<Member>())
                .Select(member => new MemberDetail
                {
                    Id = member.Object.Id,
                    Role = member.Object.Role,
                    Name = member.Object.Name,
                    Avatar = member.Object.Avatar,
                    AvatarColor = member.Object.AvatarColor,
                    Email = member.Object.Email,
                    Description = member.Object.Description,
                    ProfileImage = member.Object.ProfileImage,
                    PhoneNumber = member.Object.PhoneNumber,
                    NumPosts = member.Object.NumPosts,
                    NumMembers = member.Object.NumMembers
                }).Where(a => a.Email == user.Email).FirstOrDefault();

            return authUser;
        }
        public static async Task DeleteMemberAsync(Member model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var member = (await client.Child("Members")
                .OnceAsync<Member>()).FirstOrDefault
                (a => a.Object.Id == model.Id);

            await client.Child("Members").Child(member.Key).DeleteAsync();
        }
        public static async Task UpdateMemberAsync(MemberDetail model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var cancelToken = new CancellationTokenSource();
            var firebaseStorage = new FirebaseStorage(BloggyConstant.UploadConnString,
                new FirebaseStorageOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken), ThrowOnCancel = true });

            var toUpdateMember = (await client.Child("Members")
            .OnceAsync<Member>()).FirstOrDefault
            (a => a.Object.Id == model.Id);

            var user = toUpdateMember.Object;
            user.Name = model.Name;
            user.Description = model.Description;
            user.PhoneNumber = model.PhoneNumber;
            user.Avatar = model.Name.ToUpper().FirstOrDefault().ToString();

            if (model.ImageArray != null)
            {
                var filename = Guid.NewGuid() + ".png";
                Stream fs = new MemoryStream(model.ImageArray);
                var task = await firebaseStorage.Child("images/profiles").Child(filename).PutAsync(fs, cancelToken.Token);
                user.ProfileImage = await firebaseStorage.Child("images/profiles").Child(filename).GetDownloadUrlAsync();
            }

            await client.Child("Members").Child(toUpdateMember.Key).PutAsync(user);
        }

        #region posts
        public static async Task CreatePostAsync(PostDetail model)
        {

            FirebaseAuthLink token = await GetRefreshLink();
            var cancelToken = new CancellationTokenSource();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var firebaseStorage = new FirebaseStorage(BloggyConstant.UploadConnString,
                new FirebaseStorageOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken), ThrowOnCancel = true });

            var post = new Post
            {
                Id = model.Id,
                Title = model.Title,
                Body = model.Body,
                CreatedAt = DateTime.Now,
                Likes = ",",
                PostImage = ""
            };

            if (model.ImageArray != null)
            {
                var filename = Guid.NewGuid() + ".png";
                Stream fs = new MemoryStream(model.ImageArray);
                var task = await firebaseStorage.Child("images/posts").Child(filename).PutAsync(fs, cancelToken.Token);
                post.PostImage = await firebaseStorage.Child("images/posts").Child(filename).GetDownloadUrlAsync();
            }
            await client.Child("Posts").PostAsync(post);

            var toUpdateUser = (await client.Child("Members")
                    .OnceAsync<Member>()).FirstOrDefault
                    (a => a.Object.Role == Constant.AdminRole);

            var user = toUpdateUser.Object;
            user.NumPosts += 1; // await GetNumPostsAsync();

            await client.Child("Members").Child(toUpdateUser.Key).PutAsync(user);
        }
        public static async Task<PostDetail> GetPostByIdAsync(Guid postId)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var member = (await client.Child("Members")
                    .OnceAsync<Member>()).FirstOrDefault
                    (a => a.Object.Role == Constant.AdminRole);

            var post = (await client.Child("Posts").OnceAsync<Post>())
                .Select(item => new PostDetail
                {
                    Id = item.Object.Id,
                    Title = item.Object.Title,
                    Body = item.Object.Body,
                    CreatedAt = item.Object.CreatedAt,
                    PostedAt = Constant.GetTimeMessage(item.Object.CreatedAt),
                    PostImage = item.Object.PostImage,
                    NumLikes = item.Object.NumLikes,
                    IsLikedByMe = item.Object.Likes.Contains(member.Object.Id),
                    LikeImage = item.Object.Likes.Contains(member.Object.Id) ? "liked.png" : "unliked.png",
                    NumComments = item.Object.NumComments,
                    Details = item.Object.NumLikes.ToString() + " Like(s)    " + item.Object.NumComments.ToString() + " Comment(s)",
                    ProfileImage = member.Object.ProfileImage,
                    BloggyName = member.Object.Name,
                    Avatar = member.Object.Avatar,
                    AvatarColor = member.Object.AvatarColor,
                    InfoComments = (item.Object.NumComments > 0) ? item.Object.NumComments.ToString() + " Comments" : item.Object.NumComments.ToString() + " Comment",
                    ShortTitle = (!string.IsNullOrEmpty(item.Object.Title) && item.Object.Title.Length > 50) ? item.Object.Title.Substring(0, 50) + "... " : item.Object.Title
                }).Where(c => c.Id == postId).FirstOrDefault();

            return post;
        }
        public static async Task<ObservableCollection<PostDetail>> GetAllPostsAsync(int pageNumber, int pageSize)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var member = (await client.Child("Members")
                        .OnceAsync<Member>()).FirstOrDefault
                        (a => a.Object.Role == Constant.AdminRole);

            var posts = (await client
                  .Child("Posts")
                  .OnceAsync<Post>()).Select(item => new PostDetail
                  {
                      Id = item.Object.Id,
                      Title = item.Object.Title,
                      Body = item.Object.Body,
                      CreatedAt = item.Object.CreatedAt,
                      PostedAt = Constant.GetTimeMessage(item.Object.CreatedAt),
                      PostImage = item.Object.PostImage,
                      NumLikes = item.Object.NumLikes,
                      IsLikedByMe = item.Object.Likes.Contains(member.Object.Id),
                      LikeImage = item.Object.Likes.Contains(member.Object.Id) ? "liked.png" : "unliked.png",
                      NumComments = item.Object.NumComments,
                      Details = item.Object.NumLikes.ToString() + " Like(s)    " + item.Object.NumComments.ToString() + " Comment(s)"
                  }).OrderByDescending(m => m.CreatedAt).ToList();

            var filterPosts = posts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new ObservableCollection<PostDetail>(filterPosts);
        }

        public static async Task DeletePostAsync(PostDetail model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var post = (await client.Child("Posts")
            .OnceAsync<Post>()).FirstOrDefault
            (a => a.Object.Id == model.Id);

            await client.Child("Posts").Child(post.Key).DeleteAsync();

            var toUpdateUser = (await client.Child("Members")
                    .OnceAsync<Member>()).FirstOrDefault
                    (a => a.Object.Role == Constant.AdminRole);

            var user = toUpdateUser.Object;
            user.NumPosts -= 1; // await GetNumPostsAsync();

            await client.Child("Members").Child(toUpdateUser.Key).PutAsync(user);
        }
        public static async Task UpdatePostAsync(PostDetail model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var cancelToken = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(BloggyConstant.UploadConnString,
                new FirebaseStorageOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken), ThrowOnCancel = true });

            var toUpdatePost = (await client.Child("Posts")
            .OnceAsync<Post>()).FirstOrDefault
            (a => a.Object.Id == model.Id);

            var post = toUpdatePost.Object;
            post.Title = model.Title;
            post.Body = model.Body;

            if (model.ImageArray != null)
            {
                var filename = Guid.NewGuid() + ".png";
                Stream fs = new MemoryStream(model.ImageArray);
                var task = await firebaseStorage.Child("images/posts").Child(filename).PutAsync(fs, cancelToken.Token);
                post.PostImage = await firebaseStorage.Child("images/posts").Child(filename).GetDownloadUrlAsync();
            }

            await client.Child("Posts").Child(toUpdatePost.Key).PutAsync(post);

        }

        public static async Task<int> GetNumPostsAsync()
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var posts = (await client.Child("Posts")
                  .OnceAsync<Post>()).Select(item => new Post
                  {
                      Id = item.Object.Id
                  }).ToList();

            return posts.Count();
        }
        #endregion

        #region Comments
        public static async Task CreatePostCommentAsync(CommentDetail model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            var user = await authProvider.GetUserAsync(token.FirebaseToken);

            var comment = new Comment
            {
                Id = model.Id,
                Body = model.Body,
                AddedBy = user.Email,
                MemberId = user.LocalId,
                CreatedAt = DateTime.Now,
                PostId = model.PostId
            };
            await client.Child("Comments").PostAsync(comment);

            var toUpdatePost = (await client.Child("Posts")
            .OnceAsync<Post>()).FirstOrDefault
            (a => a.Object.Id == model.PostId);

            var post = toUpdatePost.Object;
            post.NumComments += 1;
            //post.NumComments = await GetNumCommentByPostIdAsync(model.PostId);
            await client.Child("Posts").Child(toUpdatePost.Key).PutAsync(post);

        }
        public static async Task<ObservableCollection<CommentDetail>> GetAllCommentsAsync()
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var commentList = new List<CommentDetail>();


            var comments = client
            .Child("Comments")
            .AsObservable<Comment>()
            .AsObservableCollection();

            foreach (var comment in comments)
            {
                var member = (await client.Child("Members")
                    .OnceAsync<Member>()).FirstOrDefault
                    (a => a.Object.Email == comment.AddedBy);

                var com = new CommentDetail
                {
                    Id = comment.Id,
                    Body = comment.Body,
                    AddedBy = comment.AddedBy,
                    PostId = comment.PostId,
                    CreatedAt = comment.CreatedAt,
                    MemberId = comment.MemberId,
                    MemberName = member.Object.Name,
                    ProfileImage = member.Object.ProfileImage,
                    PostedAt = Constant.GetTimeMessage(comment.CreatedAt),
                    Avatar = member.Object.Avatar,
                    AvatarColor = member.Object.AvatarColor
                };

                commentList.Add(com);
            }

            return new ObservableCollection<CommentDetail>(commentList);
        }

        public static async Task<ObservableCollection<CommentDetail>> GetCommentsByPostIdAsync(Guid postId)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var commentList = new List<CommentDetail>();


            var comments = (await client.Child("Comments").OnceAsync<Comment>())
                .Select(item => new Comment
                {
                    Id = item.Object.Id,
                    Body = item.Object.Body,
                    AddedBy = item.Object.AddedBy,
                    MemberId = item.Object.MemberId,
                    CreatedAt = item.Object.CreatedAt,
                    PostId = item.Object.PostId
                }).Where(c => c.PostId == postId).ToList();

            foreach (var comment in comments)
            {
                var member = (await client.Child("Members")
                    .OnceAsync<Member>()).FirstOrDefault
                    (a => a.Object.Email == comment.AddedBy);

                var com = new CommentDetail
                {
                    Id = comment.Id,
                    Body = comment.Body,
                    AddedBy = comment.AddedBy,
                    PostId = comment.PostId,
                    CreatedAt = comment.CreatedAt,
                    MemberId = comment.MemberId,
                    MemberName = member.Object.Name,
                    ProfileImage = member.Object.ProfileImage,
                    PostedAt = Constant.GetTimeMessage(comment.CreatedAt),
                    Avatar = member.Object.Avatar,
                    AvatarColor = member.Object.AvatarColor
                };

                commentList.Add(com);
            }

            return new ObservableCollection<CommentDetail>(commentList);
        }
        public static async Task DeleteCommentAsync(Comment model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var member = (await client.Child("Comments")
                .OnceAsync<Comment>()).FirstOrDefault
                (a => a.Object.Id == model.Id);

            await client.Child("Comments").Child(member.Key).DeleteAsync();
        }
        public static async Task UpdateCommentAsync(Comment model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var member = (await client.Child("Comments")
                .OnceAsync<Comment>()).FirstOrDefault
                (a => a.Object.Id == model.Id);

            await client.Child("Comments").Child(member.Key).PutAsync(model);
        }
        public static async Task<int> GetNumCommentByPostIdAsync(Guid postId)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var comments = (await client.Child("Comments").OnceAsync<Comment>())
                .Select(item => new Comment
                {
                    Id = item.Object.Id,
                    PostId = item.Object.PostId
                }).Where(c => c.PostId == postId).ToList();

            return comments.Count();
        }
        #endregion

        #region Like posts
        public static async Task LikePostAsync(PostLike model)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            var user = await authProvider.GetUserAsync(token.FirebaseToken);

            model.LikedBy = user.Email;

            var like = (await client.Child("PostLikes")
            .OnceAsync<PostLike>()).FirstOrDefault
            (a => a.Object.LikedBy == model.LikedBy && a.Object.PostId == model.PostId);

            if (like != null)
            {
                await client.Child("PostLikes").Child(like.Key).DeleteAsync();
            }
            else
            {
                await client.Child("PostLikes").PostAsync(model);
            }

            var toUpdatePost = (await client.Child("Posts")
            .OnceAsync<Post>()).FirstOrDefault
            (a => a.Object.Id == model.PostId);

            var post = toUpdatePost.Object;
            post.NumLikes = await GetNumLikeByPostId(model.PostId);
            await client.Child("Posts").Child(toUpdatePost.Key).PutAsync(post);
        }
        public static async Task<bool> IsPostLikedByMe(Guid postId)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
            var user = await authProvider.GetUserAsync(token.FirebaseToken);

            var like = (await client.Child("PostLikes")
            .OnceAsync<PostLike>()).FirstOrDefault
            (a => a.Object.LikedBy == user.Email && a.Object.PostId == postId);

            if (like != null)
                return true;
            else
                return false;
        }

        public static async Task<int> GetNumLikeByPostId(Guid postId)
        {
            FirebaseAuthLink token = await GetRefreshLink();
            var client = new FirebaseClient(BloggyConstant.ConnString,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken) });

            var postLikes = (await client.Child("PostLikes").OnceAsync<PostLike>())
                .Select(item => new PostLike
                {
                    Id = item.Object.Id,
                    PostId = item.Object.PostId
                }).Where(c => c.PostId == postId).ToList();

            return postLikes.Count();
        }
        #endregion

        public static async Task<FirebaseAuthLink> GetRefreshLink()
        {
            var authTokenStr = Preferences.Get("BloggyToken", string.Empty);
            var token = JsonConvert.DeserializeObject<FirebaseAuthLink>(authTokenStr);

            if (token.IsExpired())
            {
                //var email = await SecureStorage.GetAsync("Bloggy_Email");
                //var password = await SecureStorage.GetAsync("Bloggy_Password");
                var email = Preferences.Get("Bloggy_Email", string.Empty);
                var password = Preferences.Get("Bloggy_Password", string.Empty);
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(BloggyConstant.WebAPIKey));
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);

                token = await auth.GetFreshAuthAsync();
                var serializedcontent = JsonConvert.SerializeObject(token);
                Preferences.Set("BloggyToken", serializedcontent);
            }
            return token;
        }
    }
}
