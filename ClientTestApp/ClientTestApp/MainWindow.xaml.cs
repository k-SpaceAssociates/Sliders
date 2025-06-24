using System.Windows;
using kSATxtCmdNETSDk;

namespace ClientTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private kSATxtCmdClient cmdClient = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        string iPAddressInput = "localhost";
        string IPAddressInput
        {
            get => iPAddressInput;
            set => IPAddressInput = value;
        }

        int port = 49215;
        int Port
        {
            get => port;
            set => port = value;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            iPAddressInput = IPAddressTextBox.Text;

            if (int.TryParse(PortTextBox.Text, out int parsedPort))
                port = parsedPort;
            else
            {
                AppendOutput("Invalid port number.");
                return;
            }
            AppendOutput("Attempting to connect...");
            //if (cmdClient.ConnectToServer("192.168.3.10", 49215))
            if (cmdClient.ConnectToServer(IPAddressInput, port))
            {
                string response = "";
                var ret = cmdClient.GetConnectResponse(ref response);
                AppendOutput($"Connected.\nResponse: {response}\nRet: {ret}");
            }
            else
            {
                AppendOutput("Failed to connect to the server.");
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string cmd = CommandInput.Text;
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                string response = "";
                var ret = cmdClient.IssueCommand(cmd, ref response);
                AppendOutput($"Ret: {ret}\nResponse:\n{response}");
                CommandInput.Clear();
            }
        }

        private void AppendOutput(string text)
        {
            OutputBox.AppendText(text + "\n");
            OutputBox.ScrollToEnd();
        }

        protected override void OnClosed(EventArgs e)
        {
            cmdClient.Close();
            base.OnClosed(e);
        }
    }
}