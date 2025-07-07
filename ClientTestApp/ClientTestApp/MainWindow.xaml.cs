using kSATxtCmdNETSDk;
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

        private readonly TcpClientViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
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
        }
        protected override void OnClosed(EventArgs e)
        {

            //client.Close();
            base.OnClosed(e);
        }

    }
}