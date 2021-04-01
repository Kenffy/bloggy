using System;
using System.Collections.Generic;
using System.Text;

namespace Bloggy.Models
{
    public class PostLike
    {
        public Guid Id { get; set; }
        public string LikedBy { get; set; }
        public Guid PostId { get; set; }
    }
}
