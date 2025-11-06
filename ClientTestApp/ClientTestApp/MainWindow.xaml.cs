using kSATxtCmdNETSDk;
using log4net;
using System.ComponentModel;
using System.Windows;

namespace ClientTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private kSATxtCmdClient cmdClient = new();
        //private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));
        private readonly TcpClientViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            log.Debug("MainWindow initialized.");
            // DataContext = this;
            // DataContext = new TcpClientViewModel();
            vm = new TcpClientViewModel();
            this.DataContext = vm;              // ✅ Connect window XAML to ViewModel

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
            log.Debug("vm.LoadAllSettings completed.");
        }
        protected override void OnClosed(EventArgs e)
        {

            //client.Close();
            log.Debug("Starting OnClosed.");
            base.OnClosed(e);
        }

    }
}