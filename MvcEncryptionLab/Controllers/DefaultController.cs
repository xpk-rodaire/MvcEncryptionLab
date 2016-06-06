using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcEncryptionLabData;

using Microsoft.AspNet.SignalR;
using RealTimeProgressBar;  

namespace MvcEncryptionLab.Controllers
{
    public class DefaultController : ApplicationController
    {
        // GET: Default
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

        [HttpPost]
        public ActionResult RunLongProcess()
        {
            ProgressHub.SendMessage("initializing and preparing", 40);

            Logger logger = new Logger();
            DAL dal = new DAL();

            dal.RunReallyLongProcess(logger);

            Guid processId = dal.GetMostRecentProcess();

            // TODO: how to return processId to client?
            return this.Json(new { processId = processId.ToString() });
        }

        [HttpPost]
        public ActionResult GetProcessUpdate(Guid processId)
        {
            DAL dal = new DAL();

            MvcEncryptionLabData.ProcessStatus status = dal.GetProcessStatus(processId);

            return this.Json(
                new
                {
                    processId = status.ProcessId,
                    userName = status.UserName,
                    percentComplete = status.PercentComplete,
                    status = status.Status
                }
            );
        }
    }
}