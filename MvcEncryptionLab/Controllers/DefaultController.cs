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
            ViewBag.PromptForKey = (SecurityUtils.UserHasEncryptionKey(userName) ? 0 : 1);
            return View();
        }

        public ActionResult PostEncryptionKey(string key)
        {
            // Re-validate key format, pass back FAIL
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

                // Check phrase not entered yet
                if (checkPhrase == null)
                {
                    return this.Json(new { status = "PromptForPassPhrase", message = String.Empty });
                }
                else
                {
                    // Display decrypted pass phrase to user for verification
                    return this.Json(new { status = "ValidatePassPhrase", message = String.Empty, key = key, phrase = checkPhrase });
                }
            }
        }

        public ActionResult PostCheckPhraseResponse(bool value)
        {
            if (value == false)
            {
                // User rejected check phrase comparison - redirect to home page
                SecurityUtils.ExpireUserEncryptionKey(userName);
                return View("Index");
            }
            return this.Json(new { status = "success" });
        }

        public ActionResult PostCheckPhrase(string value)
        {
            // Is value long enough?

            DAL dal = new DAL();
            // Use encryption key to decrypt check phrase
            dal.SetCheckPhrase(userName, value);

            return this.Json(new { status = "success" });
        }
    }
}