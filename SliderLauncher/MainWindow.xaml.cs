using SliderLauncher;
using Sliders;
using System;
using System.ComponentModel;
using System.Windows;

namespace SliderLauncher
{
    public partial class MainWindow : Window
    {
        private readonly SliderControlViewModel sliderVM;
        private readonly TcpClientViewModel? tcpVM;

        public MainWindow()
        {
            InitializeComponent();

            // Always create the UI-facing ViewModel
            sliderVM = new SliderControlViewModel();

            // Decide dummy or real mode at runtime
            // true = simulate, false = connect to TCP
            sliderVM.Dummy = false; // <-- change this to true for dummy mode

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
            if (tcpVM != null)
            {
                _ = tcpVM.ConnectAsync(); // fire-and-forget
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (tcpVM != null)
            {
                bool allowClose = tcpVM.OnClosing();
                if (!allowClose)
                {
                    e.Cancel = true;
                    return;
                }
                tcpVM.SaveAllSettings();
            }
        }
    }
}
