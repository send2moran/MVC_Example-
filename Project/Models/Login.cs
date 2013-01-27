using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class Login
    {
        public Login()
        {
            this.Username = string.Empty;
            this.SuccessLoginDate = DateTime.MinValue.ToString();
        }

        public Login(string username,DateTime logindate, string password)
        {
            this.Username = username;
            this.SuccessLoginDate = logindate.ToString();
        }

        public Login(string username, string logindate, string password):base()
        {
        }


        public string Username { get; set; }
        public string SuccessLoginDate { get; set; }
    }
}