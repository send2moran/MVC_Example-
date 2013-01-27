using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class User
    {
        public User()
        {
            this.Username = string.Empty;
            this.IsAdmin = false;
            this.IsManuallyLocked = false;
            this.Password = string.Empty;
        }

        public User(string username, bool isadmin, bool ismanuallylocked,string password)
        {
            this.Username = username;
            this.IsAdmin = isadmin;
            this.IsManuallyLocked = ismanuallylocked;
            this.Password = password;
        }

        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsManuallyLocked { get; set; }
        public string Password { get; set; }
    }
}