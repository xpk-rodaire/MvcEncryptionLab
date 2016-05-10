using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcEncryptionLabData;

namespace MvcEncryptionLab.Controllers
{
    public class DefaultController : Controller
    {
        string userName = "schampea";

        // GET: Default
        public ActionResult Index()
        {
            // Does user have security key entered?

            return View();
        }

        public ActionResult EncryptOperation()
        {
            // Does user have security key entered?
            DAL dal = new DAL();
            ViewBag.KeyExists = (dal.SecurityKeyExists() ? 1 : 0);
            ViewBag.PromptForKey = (SecurityUtils.UserHasEncryptionKey(userName) ? 0 : 1);
            return View();
        }

        public ActionResult PostSecurityKey(string key)
        {
            // Validate key format
            if (!SecurityUtils.ValidateEncryptionKeyFormat(key))
            {
                //
                // https://visualstudiomagazine.com/blogs/tool-tracker/2015/10/return-server-side-errors-ajax.aspx
                //
                return new HttpStatusCodeResult(400, "Security key format is invalid.");
            }
            else
            {
                DAL dal = new DAL();
                if (dal.SecurityKeyExists())
                {
                    if (dal.CheckSecurityKey(key))
                    {
                        SecurityUtils.SetUserEncryptionKey(userName, key);
                        return this.Json(new { status = "success" });
                    }
                    else
                    {
                        return new HttpStatusCodeResult(400, "Security key is not valid.");
                    }
                }
                else
                {
                    SecurityUtils.SetUserEncryptionKey(userName, key);
                    dal.SetSecurityKey(key);
                    return this.Json(new { status = "success" });
                }
            }
        }
    }
}