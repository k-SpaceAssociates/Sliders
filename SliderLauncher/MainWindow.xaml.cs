using log4net;
using Sliders;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SliderLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new SliderControlViewModel();
            this.Closing += MainWindow_Closing;
        }
        bool closeCheck = false;
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (DataContext is SliderControlViewModel vm)
            {
                //log.Info("Attempting to close MainWindow.");
                if (!closeCheck)
                {
                    //log.Info("Closing MainWindow");
                    return;
                }
                bool allowClose = vm.OnClosing();

                if (!allowClose)
                {
                    //log.Info("Close canceled by user.");
                    e.Cancel = true; // Prevent closing
                }
                else
                {
                    //log.Info("Closing MainWindow confirmed.");
                }
            }
        }
    }
}