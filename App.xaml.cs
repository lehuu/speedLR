using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;

namespace SpeedLR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "SpeedLR";

            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }
            else
            {
				ErrorLogger.Initialize();
				base.OnStartup(e);
            }
        }
    }

}
