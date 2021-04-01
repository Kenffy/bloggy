using System;
using System.Collections.Generic;
using System.Text;

namespace Bloggy.Models
{
    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
