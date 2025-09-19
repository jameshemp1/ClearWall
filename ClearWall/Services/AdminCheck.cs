using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace ClearWall.Services
{
    internal class AdminCheck
    {
        public static bool IsRunningAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            //returns bool of whether app is being run as administrator
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdmin()
        {
            if (!IsRunningAsAdmin())
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    Process.Start(processInfo);
                    Process.GetCurrentProcess().Kill(); //Close current process after relaunching.
                }
                catch
                {
                    //Handle exception if the user cancels request for admin rights
                    MessageBox.Show("The application must be run as an administrator.");
                    Console.WriteLine("The application must be run as an administrator.");
                    Process.GetCurrentProcess().Kill(); //Close process
                }
            }
        }
    }
}
