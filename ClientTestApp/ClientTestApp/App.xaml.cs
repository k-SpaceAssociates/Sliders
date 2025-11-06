using System.Configuration;
using System.Data;
using System.Windows;
using log4net;
using log4net.Config;
using System.IO;

namespace ClientTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Ensure the log directory exists before log4net starts writing
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "ClientTestApp");

                Directory.CreateDirectory(logDir);
            }
            catch (Exception ex)
            {
                // If directory creation fails, you can optionally show a message or fallback
                MessageBox.Show($"Unable to create log directory:\n{ex.Message}",
                                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            log.Info("Application is starting...");
        }
    }

}
