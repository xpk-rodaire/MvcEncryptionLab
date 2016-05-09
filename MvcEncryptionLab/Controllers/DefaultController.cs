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
            ViewBag.CheckPhrase = (dal.IsCheckPhrase() ? 0 : 1);
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
                DAL dal = new DAL();
                // Use encryption key to decrypt check phrase
                string checkPhrase = dal.GetCheckPhrase(key);

                // Check phrase not entered yet
                if (checkPhrase == null)
                {
                    return this.Json(new { status = "PromptForCheckPhrase", message = String.Empty });
                }
                else
                {
                    // Display decrypted pass phrase to user for verification
                    return this.Json(new { status = "ValidateCheckPhrase", message = String.Empty, key = key, phrase = checkPhrase });
                }
            }
        }

        public ActionResult PostCheckPhrase(string value)
        {
            // Is value long enough?

            DAL dal = new DAL();
            // Use encryption key to decrypt check phrase
            dal.SetCheckPhrase(userName, value);

            return this.Json(new { status = "success" });
        }

        public ActionResult PostCheckPhraseResponse(string key)
        {
            // User rejected check phrase comparison - redirect to home page
            SecurityUtils.SetUserEncryptionKey(userName, key);
            return this.Json(new { status = "success" });
        }

        public ActionResult PostSecurityItems(string key, string phrase)
        {
            DAL dal = new DAL();
            bool isCheckPhrase = dal.IsCheckPhrase();

            // Error if already a check phrase
            if (isCheckPhrase != string.IsNullOrEmpty(phrase))
            {
                return new HttpStatusCodeResult(400, "Error processing check phrase.");
            }

            // Validate phrase
            //if (2 ! 3)
            //{
            //    return new HttpStatusCodeResult(400, "Invalid phrase format");
            //}

            // Validate key
            if (!SecurityUtils.ValidateEncryptionKeyFormat(key))
            {
                //
                // https://visualstudiomagazine.com/blogs/tool-tracker/2015/10/return-server-side-errors-ajax.aspx
                //
                return new HttpStatusCodeResult(400, "Invalid security key format.");
            }

            // Use encryption key to decrypt check phrase



            string checkPhrase = dal.GetCheckPhrase(key);

            {
                // Display decrypted pass phrase to user for verification
                return this.Json(new { status = "ValidateCheckPhrase", message = String.Empty, key = key, phrase = checkPhrase });
            }

            return RedirectToAction("Index"); 
        }
    }
}