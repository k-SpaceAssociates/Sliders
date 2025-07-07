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

        //string iPAddressInput = "localhost";
        //string IPAddressInput
        //{
        //    get => iPAddressInput;
        //    set => iPAddressInput = value;
        //}

        //int port = 49215;
        //int Port
        //{
        //    get => port;
        //    set => port = value;
        //}

        //private void Connect_Click(object sender, RoutedEventArgs e)
        //{
        //    IPAddressInput = IPAddressTextBox.Text;

        //    if (int.TryParse(PortTextBox.Text, out int parsedPort))
        //        Port = parsedPort;
        //    else
        //    {
        //        AppendOutput("Invalid port number.");
        //        return;
        //    }
        //    AppendOutput("Attempting to connect...");
        //    //if (cmdClient.ConnectToServer("192.168.3.10", 49215))
        //    if (cmdClient.ConnectToServer(IPAddressInput, Port))
        //    {
        //        string response = "";
        //        var ret = cmdClient.GetConnectResponse(ref response);
        //        AppendOutput($"Connected.\nResponse: {response}\nRet: {ret}");
        //    }
        //    else
        //    {
        //        AppendOutput("Failed to connect to the server.");
        //    }
        //}

        //private void Send_Click(object sender, RoutedEventArgs e)
        //{
        //    string cmd = CommandInput.Text;
        //    if (!string.IsNullOrWhiteSpace(cmd))
        //    {
        //        string response = "";
        //        var ret = cmdClient.IssueCommand(cmd, ref response);
        //        AppendOutput($"Ret: {ret}\nResponse:\n{response}");
        //        CommandInput.Clear();
        //    }
        //}

        //private void AppendOutput(string text)
        //{
        //    OutputBox.AppendText(text + "\n");
        //    OutputBox.ScrollToEnd();
        //}

        //protected override void OnClosed(EventArgs e)
        //{
        //    cmdClient.Close();
        //    base.OnClosed(e);
        //}
    }
}