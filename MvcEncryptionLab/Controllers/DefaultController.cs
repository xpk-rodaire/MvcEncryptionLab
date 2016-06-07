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
        public static void SendProgressMessageDelegateMethod(string text, int percentComplete)
        {
            ProgressHub.SendMessage(text, percentComplete);
        }

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
            //foreach (int index in Enumerable.Range(1, 10))
            //{
            //    ProgressHub.SendMessage("Processed next 1000 records " + index, index * 10);
            //    Thread.Sleep(2000);
            //}

            DAL dal = new DAL();

            dal.RunReallyLongProcess(SendProgressMessageDelegateMethod);

            //Guid processId = dal.GetMostRecentProcess();

            // TODO: how to return processId to client?
            //return this.Json(new { processId = processId.ToString() });
            return this.Json(new { processId = 12345 });
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