using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcEncryptionLab.Controllers
{
    public class Default2Controller : ApplicationController
    {
        // GET: Default2
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EncryptOperation()
        {
            this.GetSecurityKey();
            return View();
        }

        public ActionResult NoEncryptOperation()
        {
            return View();
        }
    }
}