using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FipsCheckerUtility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form = new Form1();

            try
            {
                System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
                form.DisplayText = String.Format("This system is NOT FIPS compliant.");
            }
            catch (Exception e)
            {
                string fipsErrorMessage = "This implementation is not part of the Windows Platform FIPS validated cryptographic algorithms";

                if (e.Message.Contains(fipsErrorMessage)
                    || (e.InnerException != null && e.InnerException.Message.Contains(fipsErrorMessage)))
                {
                    form.DisplayText = String.Format("This system is FIPS compliant.");
                }
                else
                {
                    form.DisplayText = String.Format("Error checking FIPS compliance: {0}", e.Message);
                }
            }
            Application.Run(form);
        }
    }
}
