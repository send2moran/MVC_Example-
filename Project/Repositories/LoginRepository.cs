using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Project.Models;
using System.Collections.Concurrent;
using System.Configuration;

namespace Project.Repositories
{
    public class LoginRepository :ILoginRepository
    {
        public string LOGIN_PATH = HttpContext.Current.Server.MapPath("~/App_Data/Logins.xml");
        //safe thread approach to handle concurrency

        //handles multiple threads. This type from the System.Collections.
        //Concurrent namespace allows multiple threads to access a Dictionary instance.
        //With ConcurrentDictionary, you get a hash-based lookup algorithm that is safe to use on multiple threads

        private ConcurrentDictionary<string, Login> allLogins = new ConcurrentDictionary<string, Login>();
        private XDocument LoginsData;


        public LoginRepository()
        {
            Load();
        }

        public void Load()
        {
            LoginsData = XDocument.Load(LOGIN_PATH);

            var logins = from lgs in LoginsData.Document.Descendants("Login")
                         select new Login
                         {
                             Username = lgs.Element("Username").Value,
                             SuccessLoginDate = lgs.Element("SuccessLoginDate").Value
                         };
            foreach (var item in logins)
            {
                allLogins.TryAdd(item.Username, item);
            }
        }

        public void Save()
        {
            LoginsData = new XDocument();
            LoginsData.Document.Add(new XElement("LoginRepository"));

            foreach (var item in allLogins)
            {
                XElement root = new XElement("Login");
                root.Add(new XElement("Username", item.Value.Username),
                new XElement("SuccessLoginDate", item.Value.SuccessLoginDate));
                LoginsData.Root.Add(root);
            }
            LoginsData.Save(LOGIN_PATH);

        }

        public List<Login> GetLogin()
        {
            return allLogins.Select(p=>p.Value).ToList();
        }

        public Models.Login GetLoginByID(string userid)
        {
            return allLogins.SingleOrDefault(p => p.Key == userid).Value;
        }

        public Models.Login InsertLogin(Models.Login login)
        {
            allLogins.TryAdd(login.Username, login);
            Save();
            return login;
        }

        public bool DeleteLogin(Models.Login login)
        {
            Login l = new Login();
            allLogins.TryRemove(login.Username,out l);
            Save();
            return (l != null);
        }


        public void SetSuccessLogin(Models.Login login)
        {
            login = GetLoginByID(login.Username);
            Models.Login newLogin = new Login() { SuccessLoginDate = DateTime.Now.ToString(), Username = login.Username };
            allLogins.TryUpdate(login.Username, newLogin,login);
            Save();
        }

        
    }
}