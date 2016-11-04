using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcEncryptionLabData;

using Microsoft.AspNet.SignalR;
using RealTimeProgressBar;
using System.Threading;  

namespace MvcEncryptionLab.Controllers
{
    public class DefaultController : ApplicationController
    {
        // GET: Default
        public ActionResult Index(LogItem logItem)
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

        [HttpPost]
        public ActionResult RunLongProcess()
        {
            DAL dal = new DAL();
            LogItem logItem = null;
            dal.RunReallyLongProcess(SendProgressMessageDelegateMethod, out logItem);
            ViewBag.Message = logItem.Text;
            return View("Index");
        }
    }
}