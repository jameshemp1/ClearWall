using ClearWall.Services;
using System.Windows;

namespace ClearWall
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AdminCheck.RestartAsAdmin();
        }
    }

}
