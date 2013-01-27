using Project.Helpers;
using Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/
        private ILoginRepository _repository;

        public DefaultController(ILoginRepository repository)
        {
            _repository = repository;
        }

        [Authorize]
        [LockAuthorizationAttribute]
        public ActionResult Index()
        {
            var listOFLogins = _repository.GetLogin().ToList();
            return View(listOFLogins);
        }

    }
}
