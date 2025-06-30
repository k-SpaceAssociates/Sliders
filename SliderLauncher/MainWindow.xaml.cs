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

        private readonly TcpClientViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            // DataContext = this;
            // DataContext = new TcpClientViewModel();
            vm = new TcpClientViewModel();
            this.DataContext = vm;              // ✅ Connect window XAML to ViewModel
            this.sliderControl.Loaded += (s, e) =>
            {
                sliderControl.DataContext = vm.SliderVM;
                Debug.WriteLine("sliderControl.DataContext assigned in Loaded event.");
            }; // ✅ Connect sliderControl to same ViewModel
           // DataContext = vm;
           // sliderControl.DataContext = vm.SliderVM;

            //AutoLaunch = Properties.Settings.Default.AutoLaunch;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        bool closeCheck = false;


        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            bool allowClose = vm.OnClosing();
            if (!allowClose)
            {
                e.Cancel = true;
                return;
            }
            vm.SaveAllSettings();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            vm.LoadAllSettings();
        }
        protected override void OnClosed(EventArgs e)
        {

            //client.Close();
            base.OnClosed(e);
        }
    }
}