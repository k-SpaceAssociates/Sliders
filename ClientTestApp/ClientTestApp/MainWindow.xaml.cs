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

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            AppendOutput("Attempting to connect...");

            if (cmdClient.ConnectToServer("localhost", 49215))
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