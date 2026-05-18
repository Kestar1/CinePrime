using System;
using System.Windows.Forms;
using CinePrime.BLL.Models;
using CinePrime.UI.Forms;

namespace CinePrime.UI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, args) =>
            {
                UiFeedback.ShowException(null, args.Exception, "A aparut o eroare neasteptata in interfata.");
            };
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                UiFeedback.ShowException(null, args.ExceptionObject as Exception, "A aparut o eroare critica in aplicatie.");
            };

            var services = new ServiceRegistry();
            Application.Run(new LoginForm(services));
        }
    }
}
