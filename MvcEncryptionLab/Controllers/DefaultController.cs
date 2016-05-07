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
            ViewBag.PromptForKey = (SecurityUtils.UserHasEncryptionKey(userName) ? 0 : 1);
            return View();
        }

        public ActionResult PostEncryptionKey(string key)
        {
            // Re-validate key, pass back FAIL
            if (!SecurityUtils.ValidateEncryptionKeyFormat(userName, key))
            {
                //
                // https://visualstudiomagazine.com/blogs/tool-tracker/2015/10/return-server-side-errors-ajax.aspx
                //
                return new HttpStatusCodeResult(400, "Ajax error test");
            }
            else
            {
                SecurityUtils.SetUserEncryptionKey(userName, key);

                DAL dal = new DAL();
                // Use encryption key to decrypt check phrase
                string checkPhrase = dal.GetCheckPhrase(userName);

                // Display decrypted pass phrase to user for verification

                return this.Json(new { status = "success", checkPhrase = checkPhrase });
            }
        }

        public ActionResult PostCheckPhraseResponse(bool value)
        {
            return null;
        }
    }
}