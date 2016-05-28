using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcEncryptionLabData;

namespace MvcEncryptionLab.Controllers
{
    public abstract class ApplicationController : Controller
    {
        private string User
        {
            get
            {
                // return HttpContext.User.Identity.Name;
                return "schampea";
            }
        }

        public ApplicationController()
        {
        }

        protected void GetSecurityKey()
        {
            // Does user have security key entered?
            DAL dal = new DAL();
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            ViewBag.KeyExists = (dal.SecurityKeyExists() ? 1 : 0);
            ViewBag.PromptForKey = (SecurityUtils.UserHasEncryptionKey(User) ? 0 : 1);
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
                        SecurityUtils.SetUserEncryptionKey(User, key, 10);
                        return this.Json(new { status = "success" });
                    }
                    else
                    {
                        return new HttpStatusCodeResult(400, "Security key is not valid.");
                    }
                }
                else
                {
                    SecurityUtils.SetUserEncryptionKey(User, key, 5);
                    dal.SetSecurityKey(key);
                    return this.Json(new { status = "success" });
                }
            }
        }
    }
}