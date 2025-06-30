using Bluegrams.Application;
using SliderLauncher.Properties;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace SliderLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigurePortableSettings();

        }

        private void ConfigurePortableSettings()
        {
            string settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Sliders");

            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            PortableSettingsProvider.SettingsFileName = "sliders.config";
            PortableSettingsProvider.SettingsDirectory = settingsPath;

            // Important: Replace the provider *before* Settings.Default is used
            Settings.Default.Providers.Clear();
            Settings.Default.Providers.Add(new PortableSettingsProvider());

            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                property.Provider = Settings.Default.Providers["PortableSettingsProvider"];
            }

            try
            {
                Settings.Default.Reload(); // Load settings from the custom file
            }
            catch (ConfigurationErrorsException ex)
            {
                MessageBox.Show($"Failed to load settings:\n{ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }

}
