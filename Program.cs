using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAMBuddy2
{


    static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Utility.GetRootNodeText(@"C:\TD\CSDFK-1971\local\WIP\a child.oet");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainForm());
            } catch (Exception e)
            {
                
                Logger.Error(e, "An error occurred which is going to cause the application to close.");
            }
        }
    }
}
