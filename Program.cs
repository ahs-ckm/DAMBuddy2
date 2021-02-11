using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            Logger.Log(NLog.LogLevel.Info, "Startup");
            const string appName = "BuildBuddy";
            bool createNew = false;
            //Utility.GetRootNodeText(@"C:\TD\CSDFK-1971\local\WIP\a child.oet");
            using (new Semaphore(0, 1, appName, out createNew))
            {
                if (createNew)
                {
                    Console.WriteLine("One instance of MyApplication is created and running...");
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    try
                    {
                        Application.Run(new MainForm());
                    }
                    catch (Exception e)
                    {

                        Logger.Error(e, "An error occurred which is going to cause the application to close.");
                    }

                }
                else
                {
                    MessageBox.Show($"Only one instance of {appName} can be running at a time.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }
    }
}
