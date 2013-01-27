using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Models;

namespace Project.Repositories
{
    public interface ILoginRepository
    {
        List<Login> GetLogin();
        Login GetLoginByID(string userid);
        Models.Login InsertLogin(Login login);
        bool DeleteLogin(Login login);
        void SetSuccessLogin(Login login);
        void Save();
        void Load();
    }
}
