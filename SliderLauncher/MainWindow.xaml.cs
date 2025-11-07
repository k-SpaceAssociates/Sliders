using log4net;
using SliderLauncher;
using Sliders;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace SliderLauncher
{
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));
        private readonly SliderControlViewModel sliderVM;
        private readonly TcpClientViewModel? tcpVM;

        public MainWindow()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title = $"Sliders v{version}";
            log.Debug("MainWindow initialized.");
            // Always create the UI-facing ViewModel
            sliderVM = new SliderControlViewModel();

            // Decide dummy or real mode at runtime
            // true = simulate, false = connect to TCP
            //sliderVM.Dummy = true; // <-- change this to true for dummy mode

            // Bind slider control's DataContext to the UI ViewModel
            sliderControl.DataContext = sliderVM;

            if (!sliderVM.Dummy)
            {
                // Pass the UI ViewModel to TcpClientViewModel so it can push updates
                tcpVM = new TcpClientViewModel(sliderVM);
                DataContext = tcpVM;
            }

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (tcpVM != null && tcpVM.AutoLaunch == true)
            {
                _ = tcpVM.ConnectAsync(); // fire-and-forget
                log.Debug("tcpVM.ConnectAsync(); completed.");
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            log.Debug("Starting MainWindow_Closing.");
            bool? status = sliderVM?.OnClosing();
            if (status != null && status == false)
            {
                e.Cancel = true;
                return;
            }
                
            if (tcpVM != null)
            {
                bool allowClose = tcpVM.OnClosing();
                if (!allowClose)
                {
                    e.Cancel = true;
                    return;
                }
                tcpVM.SaveAllSettings();
                log.Debug("tcpVM.SaveAllSettings completed.");
            }
        }
    }
}
