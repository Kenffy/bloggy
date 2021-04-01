using System;
using System.Collections.Generic;
using System.Text;

namespace Bloggy.Models
{
    public class MemberDetail
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string AvatarColor { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string ProfileImage { get; set; }
        public string PhoneNumber { get; set; }
        public int NumPosts { get; set; }
        public int NumMembers { get; set; }

        public byte[] ImageArray { get; set; }
        public string ShortDesc { get; set; }
    }
}
