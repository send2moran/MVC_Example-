using Project.Helpers;
using Project.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Project.Controllers
{
    public class LoginController : Controller
    {

         private IUserRepository _userRepository;
         private ILoginRepository _loginRepository;

        public LoginController(IUserRepository userRepository, ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
            _userRepository = userRepository;
        }


        //
        // GET: /Login/
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Enter()
        {
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Logout()
        {
            TempData["loginAttempt"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }



        [HttpPost]
        [Throttle]
        public ActionResult Enter(FormCollection collection)
        {
            ViewData["configRetry"] = Convert.ToInt32(ConfigurationManager.AppSettings.Get("LoginAttempts"));
            Models.User user = new Models.User();
            Models.Login loginState = new Models.Login();

            string username = collection["Username"];
            string password = collection["Password"];


            user = _userRepository.GetUserByID(username);
            loginState = _loginRepository.GetLoginByID(username);

            // if user or it's loginstate is null - user is not found.
            if (user == null || loginState == null)
            {
                ViewData["NotFound"] = "User not found";
            }
            else
            {
                //if unlocked - log in
                if (!user.IsManuallyLocked)
                {
                    //login attempt - if succeded - singout current logged in user
                    //remove loginattempt tempdata
                    //set auth cookie for new login.
                    //set attempt cookie to an empty string
                    //refirect to main page.
                    if (user.Password == password)
                    {
                        FormsAuthentication.SignOut();
                        _loginRepository.SetSuccessLogin(loginState);
                        TempData["loginAttempt"] = null;
                        FormsAuthentication.SetAuthCookie(username, true);
                        HttpContext.Response.Cookies["logintry_" + user.Username].Value = "";
                        return RedirectToRoute(new
                        {
                            controller = "Default",
                            action = "Index",
                            userId = username
                        });
                    }
                }
                else //if unlocked - can't log in until unlocked by Admin
                {
                    TempData["IsManuallyLocked"] = "you are locked by the admin !";
                }
            }
            
            return View("index");
        }



    }
}
