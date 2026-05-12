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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceRegistry();
            Application.Run(new LoginForm(services));
        }
    }
}
