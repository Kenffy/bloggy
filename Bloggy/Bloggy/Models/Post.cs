using System;
using System.Collections.Generic;
using System.Text;

namespace Bloggy.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PostImage { get; set; }
        public int NumLikes { get; set; }
        public int NumComments { get; set; }
    }
}
