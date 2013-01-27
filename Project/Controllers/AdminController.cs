using Project.Helpers;
using Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        private readonly ILoginRepository _loginRepository;
       private readonly IUserRepository _userRepository;

        public AdminController(ILoginRepository loginRepository, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _loginRepository = loginRepository;
        }

        [Authorize]
        [AdminAuthorizationAttribute]
        [LockAuthorizationAttribute]
        public ActionResult Index()
        {
            var listOfUsers = _userRepository.GetUsers().ToList();
            return View(listOfUsers);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return RedirectToAction("Index");
        }
        // POST: /Admin/Create

        [HttpPost]
        [Authorize]
        [AdminAuthorizationAttribute]
        [LockAuthorizationAttribute]
        public ActionResult Create(FormCollection collection)
        {
            if (_userRepository.GetUserByID(collection["username"]) == null)
            {
                _userRepository.InsertUser(new Models.User() { Username = collection["username"], Password = collection["password"] });
                _loginRepository.InsertLogin(new Models.Login() { SuccessLoginDate = string.Empty, Username = collection["username"] });
            }
            else
            {
                TempData["UserExist"] = true;
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [AdminAuthorizationAttribute]
        [LockAuthorizationAttribute]
        public ActionResult MakeAdmin(string id)
        {
            Models.User u = _userRepository.GetUserByID(id);
            u.IsAdmin = (u.IsAdmin) ? false : true;
            _userRepository.UpdateUser(u);
            return RedirectToAction("Index");
        }

        [Authorize]
        [AdminAuthorizationAttribute]
        [LockAuthorizationAttribute]
        public ActionResult Delete(string id)
        {
            _userRepository.DeleteUser(id);
            return RedirectToAction("Index");
        }

        
        [Authorize]
        [AdminAuthorizationAttribute]
        [LockAuthorizationAttribute]
        public ActionResult Lockuser(string id)
        {
            Models.User u = _userRepository.GetUserByID(id);
            u.IsManuallyLocked = (u.IsManuallyLocked) ? false :true;
            _userRepository.UpdateUser(u);
            return RedirectToAction("Index");
        }



    }
}
