using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Models;

namespace Project.Repositories
{
    public interface IUserRepository
    {
        List<User> GetUsers();
        User GetUserByID(string userid);
        User InsertUser(User user);
        void UpdateUser(User user);
        bool DeleteUser(string username);
        void LockUser(User user);
        void Save();
        void Load();
    }
}
