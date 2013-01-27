using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Project.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Project.Helpers;

namespace Project.Repositories
{
    public class UserRepository : IUserRepository
    {
        public string USERS_PATH = HttpContext.Current.Server.MapPath("~/App_Data/Users.xml");
        //safe thread approach to handle concurrency
        
        //handles multiple threads. This type from the System.Collections.
        //Concurrent namespace allows multiple threads to access a Dictionary instance.
        //With ConcurrentDictionary, you get a hash-based lookup algorithm that is safe to use on multiple threads

        private ConcurrentDictionary<string, User> allUsers = new ConcurrentDictionary<string, User>();
        private XDocument UsersData;


        public UserRepository()
        {
            Load();
        }


        public void Save()
        {
            UsersData = new XDocument();
            UsersData.Document.Add(new XElement("UserRepository"));
            
            foreach (var item in allUsers)
            {
                XElement root = new XElement("User");
                root.Add(new XElement("Username", item.Value.Username),
                new XElement("IsAdmin", item.Value.IsAdmin),
                new XElement("Password", PasswordHelper.EnryptString(item.Value.Password)),
                new XElement("IsManuallyLocked", item.Value.IsManuallyLocked));
                UsersData.Root.Add(root);
            }
            UsersData.Save(USERS_PATH);
        }

        public void Load()
        {
            UsersData = XDocument.Load(USERS_PATH);

            var users = from u in UsersData.Document.Descendants("User")
                        select new User()
                        {
                            Username = u.Element("Username").Value,
                            IsAdmin = (bool)u.Element("IsAdmin"),
                            Password = PasswordHelper.DecryptString(u.Element("Password").Value),
                            IsManuallyLocked = (bool)u.Element("IsManuallyLocked")
                        };
            foreach (var item in users)
            {
                allUsers.TryAdd(item.Username, item);
            }
        }



        public List<User> GetUsers()
        {
            return allUsers.Select(p => p.Value).ToList();
        }

        public Models.User GetUserByID(string userid)
        {
            return allUsers.SingleOrDefault(p => p.Key == userid).Value;
        }

        public Models.User InsertUser(Models.User user)
        {
            allUsers.TryAdd(user.Username, user);
            Save();
            return user;
        }


        public void UpdateUser(Models.User user)
        {
            user.Password = user.Password;
            allUsers.TryUpdate(user.Username, user, GetUserByID(user.Username));
            Save();
        }


        public bool DeleteUser(string username)
        {
            User u = new User();
            allUsers.TryRemove(username, out u);
            Save();
            return (u != null);
        }

        public void LockUser(Models.User user)
        {
            user.IsManuallyLocked = true;
            user.Password =user.Password;
            allUsers.TryUpdate(user.Username, user, GetUserByID(user.Username));
            Save();
        }




        
    }
}