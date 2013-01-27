using Project.Controllers;
using Project.Models;
using Project.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;

namespace Project.Helpers
{
    /// <summary>
    /// admin only filter
    /// </summary>
    public class AdminAuthorizationAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
            IUserRepository _userRepository = DependencyResolver.Current.GetService<IUserRepository>();
            Models.User u = _userRepository.GetUserByID(filterContext.HttpContext.User.Identity.Name);
            if (u != null)
            {
                if (!u.IsAdmin)
                {
                    filterContext.Controller.TempData["AdminOnly"] = "This section is for Admin Only !";
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new  {Controller = "Login" }));
                    filterContext.Canceled = true;

                }
            }
            base.OnActionExecuted(filterContext);
        }
    }


    /// <summary>
    /// lock mode filter
    /// </summary>
    public class LockAuthorizationAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
            IUserRepository userRepository = DependencyResolver.Current.GetService<IUserRepository>();
            Models.User u = userRepository.GetUserByID(filterContext.HttpContext.User.Identity.Name);
            if(u != null){
                if (u.IsManuallyLocked)
                {
                    filterContext.Controller.TempData["IsManuallyLocked"] = "you are locked by the admin !";
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { Controller = "Login" }));
                    filterContext.Canceled = true;
                }
            }
            base.OnActionExecuted(filterContext);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ThrottleAttribute : ActionFilterAttribute
    {

        private ActionExecutingContext fContext;

        /// <summary>
        /// set attempt value in a cookie per username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="attempt"></param>
        private void setTryCookie(string username, int attempt)
        {
            HttpCookie myCookie = (fContext.HttpContext.Request.Cookies["logintry_" + username]) ?? new HttpCookie("logintry_" + username);
            myCookie.Value = attempt.ToString();
            myCookie.Expires = DateTime.Now.AddDays(1);
            fContext.HttpContext.Response.Cookies.Add(myCookie);
        }

        /// <summary>
        /// get the attempt value from cookie per username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private int getTryCookie(string username)
        {
            int attempt = 0;
            HttpCookie myCookie = (fContext.HttpContext.Request.Cookies["logintry_" + username]) ?? new HttpCookie("logintry_" + username);
            return int.TryParse(myCookie.Value,out attempt) ? attempt : 0;
        }


        private ILoginRepository loginRepository = DependencyResolver.Current.GetService<ILoginRepository>();
        private IUserRepository userRepository = DependencyResolver.Current.GetService<IUserRepository>();


        /// <summary>
        /// Cache callback function to indicate captcha state,
        /// after lockdown.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="removedReason"></param>
        public void ReportRemovedCallback(String key, object value,CacheItemRemovedReason removedReason)
        {
            HttpRuntime.Cache.Add(((Login)value).Username + "_captcha",
                        true,
                        null,
                        DateTime.Now.AddHours(1),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
        }


        /// <summary>
        /// the attempts are stored inside a cookie per username
        /// after user reach max login attempts as set in the configuration 
        /// the user will be blocked for 1 min - using the cache system.
        /// when the block ends (cache expires) a new cache entry will be added indicating
        /// the captcha stage.
        /// after complete the captcha stage the user will be loged in (if succeded) or
        /// the system will set new attempt entry for him.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            fContext = filterContext;

            var id = ((FormCollection)filterContext.ActionParameters["collection"])["Username"];
            var captcha = ((FormCollection)filterContext.ActionParameters["collection"])["Captcha"];

            Models.Login login = loginRepository.GetLoginByID(id); // get new updated data from repo
            Models.User user = userRepository.GetUserByID(id); // get new updated data from repo

            if (string.IsNullOrEmpty(id) && user != null && user.IsManuallyLocked) return;

            int loginAttemptsConfig = Convert.ToInt32(ConfigurationManager.AppSettings.Get("LoginAttempts"))+1;
            
            if (login == null) return;


            int attemptFromCookie = getTryCookie(login.Username);

            //reaching captcha state , -1 will be that indication.
            if (HttpRuntime.Cache[login.Username + "_captcha"] != null)
            {
                attemptFromCookie= -1;
                setTryCookie(login.Username, attemptFromCookie);
                HttpRuntime.Cache.Remove(login.Username + "_captcha");
            }
            

            //captcha submited, captcha validation.
            if (!string.IsNullOrEmpty(captcha) && CaptchaController.IsValidCaptchaValue(captcha))
            {
                attemptFromCookie = 1;
                setTryCookie(login.Username, attemptFromCookie);
            }

            //setting values and redirecting for captcha state
            if (attemptFromCookie == -1)
            {
                filterContext.Controller.TempData["useCaptcha"] = true;
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { Controller = "Login", Action = "index" }));
                return;
            }

            //normal attmept cycle, the user will get +1 in the attempts cookie
            if (attemptFromCookie < loginAttemptsConfig)
            {
                attemptFromCookie = attemptFromCookie + 1;
                setTryCookie(login.Username, attemptFromCookie);
                filterContext.Controller.TempData["loginAttempt"] = attemptFromCookie;
            }


            //reaching max atttempts available,
            //set key for the cache.
            var key = string.Concat("BlockIP-", filterContext.HttpContext.Request.UserHostAddress);

            if (attemptFromCookie >= loginAttemptsConfig)
            {
                bool allowExecute = false;

                //still blocked - setting values and redirection.
                if (!allowExecute)
                {
                    string Message = "You are blocked for 1 minute !";
                    filterContext.Controller.TempData["Blocked"] = Message;
                    filterContext.Controller.TempData["useCaptcha"] = false;
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { Controller = "Login", Action = "index" }));
                }

                if (HttpRuntime.Cache[key] == null)
                {
                    HttpRuntime.Cache.Add(key,
                        login,
                        null,
                        DateTime.Now.AddMinutes(1),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        new CacheItemRemovedCallback(ReportRemovedCallback));
                    allowExecute = true;
                }
                
            }

            
        }
    }


}