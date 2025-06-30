using kSATxtCmdNETSDk;
using log4net;
using SliderLauncher;
using Sliders;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Quic;
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
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Sockets;
using System.Threading.Tasks;



namespace SliderLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            // DataContext = this;
            DataContext = new TcpClientViewModel();
            //AutoLaunch = Properties.Settings.Default.AutoLaunch;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        bool closeCheck = false;
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (DataContext is TcpClientViewModel vm2)
            {
                bool allowClose = vm2.OnClosing(); // calls inner SliderControlViewModel.OnClosing()

                if (!allowClose)
                {
                    e.Cancel = true;
                    return;
                }
                vm2.SaveAllSettings();
            }

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TcpClientViewModel vm2)
            {
                vm2.LoadAllSettings();
                //if (vm2.AutoLaunch)
                //    vm2.ConnectCommand.Execute(null); // if auto-launch logic is needed
            }
        }
        protected override void OnClosed(EventArgs e)
        {

            //client.Close();
            base.OnClosed(e);
        }
    }
}