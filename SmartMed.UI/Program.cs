using System;
using System.Windows.Forms;
using SmartMed.Common.Exceptions;
using SmartMed.UI.Bootstrap;

namespace SmartMed.UI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var bootstrapper = new ApplicationBootstrapper();
                Application.Run(bootstrapper.BuildMainForm());
            }
            catch (AuthenticationException exception)
            {
                MessageBox.Show(
                    exception.Message,
                    "SmartMed Authentication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (AppException exception)
            {
                MessageBox.Show(
                    exception.Message,
                    "SmartMed Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
