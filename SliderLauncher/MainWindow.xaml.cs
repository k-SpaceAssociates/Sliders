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

namespace SliderLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private readonly DispatcherTimer _timer = new();
        private readonly kSATxtCmdClient cmdClient = new();
        CommandClientHandler client = new CommandClientHandler();
        private bool _isRunning = false;
        private SliderControlViewModel vm;
        private int _stageUpdateIndex = 0;

        string iPAddressInput = "localhost";
        string IPAddressInput
        {
            get => iPAddressInput;
            set => iPAddressInput = value;
        }

        int port = 49215;
        int Port
        {
            get => port;
            set => port = value;
        }
        public MainWindow()
        {
            InitializeComponent();
            IPAddressTextBox.Text = Properties.Settings.Default.IPAddress;
            PortTextBox.Text = Properties.Settings.Default.Port.ToString();
            if (int.TryParse(PortTextBox.Text, out int parsedPort))
            {
                Port = parsedPort;
            }
            else
            {
                AppendOutput("Invalid port number.");
                return;
            }
            IPAddressInput= IPAddressTextBox.Text;
            vm = new SliderControlViewModel();
            sliderControl.DataContext = vm;
            if (!vm.Dummy)
            {
                //ClientConnect(); Auto connect on startup
                vm._stepSize =1800 / (36 / 0.05); // update every 50ms
                _timer.Interval = TimeSpan.FromMilliseconds(200);
                _timer.Tick += (s, e) => RunCommands();
                _timer.Start();
            }

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

        //public ObservableCollection<string> CommandsToRun { get; } = new()
        //{
        //    "?",
        //    "list",
        //    "direction",
        //    "position",
        //    "follow",
        //};

        //public ObservableCollection<string> StageList { get; } = new() { };
        //int maxStages = 4;
        //public ObservableCollection<string> StagePositions { get; } = new() { };

        //bool follow = false;
        //bool Follow { get; set; }

        //string? direction = "";
        //string? Direction { get; set; }
        public void RunCommands()
        {
            if (_isRunning)
                return;
            _isRunning = true;

            try
            {
                foreach (var command in vm.CommandsToRun)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        if (command == "position")
                        {
                            if (vm.StageList != null)
                            {
                                foreach (string stage in vm.StageList) //There will be no stages if connection is lost
                                {
                                    Send(command + " " + stage);
                                }
                            }
                        }
                        else
                        {
                            Send(command);
                        }
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            ClientConnect();
        }
        private void ClientConnect()
        {
            IPAddressInput = IPAddressTextBox.Text;
            Properties.Settings.Default.IPAddress = IPAddressInput;

            if (int.TryParse(PortTextBox.Text, out int parsedPort))
            {
                Port = parsedPort;
                Properties.Settings.Default.Port = Port;
            }
            else
            {
                AppendOutput("Invalid port number.");
                return;
            }
            Properties.Settings.Default.Save(); // Save the port setting
            // bool connected = client.Connect("localhost", 49215, out var response, out var ret);
            //bool connected = client.Connect("192.168.3.10", 49215, out var response, out var ret);
            bool connected = client.Connect(IPAddressInput, Port, out var response, out var ret);
            Debug.WriteLine(connected ? $"Connected.\nResponse: {response}\nRet: {ret}" : response);
        }

        private void Send(string command)
        {
            bool sent = client.Send(command, out var response, out var ret);
            Debug.WriteLine($"Ret: {ret}\nCommand: {command}\nResponse:{response}\n");
            AssignResults(command, response);
        }

        void AssignResults(string cmd, string response)
        {
            if (cmd == "list") //Assume list gets Stages available
            {
                var stages = ConvertToList(response);
                vm.StageList.Clear();
                foreach (var stage in stages)
                    vm.StageList.Add(stage);
            }
            else if (cmd.StartsWith("position", StringComparison.OrdinalIgnoreCase))
            {
                var stagePosition = cmd.Substring("position".Length).Trim();
                //if(vm.StagePositions.Count == 4) vm.StagePositions.Clear(); //This will clear current info. and start getting next 4

                var lines = response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var newValue = lines[0];

                    if (vm.StagePositions.Count < 4)
                    {
                        vm.StagePositions.Add(newValue); // Add until you have 4 items
                    }
                    else
                    {
                        vm.StagePositions[_stageUpdateIndex] = newValue; // Overwrite existing item
                    }

                    _stageUpdateIndex = (_stageUpdateIndex + 1) % 4; // Wrap index from 0 to 3
                }
                if (vm.StagePositions.Count == 4)
                    vm.UpdateSlider();
            }
            else if (cmd == "direction")
            {
                var lines = response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                    vm.Direction = lines[0];
            }

            else if (cmd == "follow")
            {
                if (response.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    vm.Follow = false;
                }
                else if (response.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    vm.Follow = true;
                }
                else vm.Follow = false;
            }
            else if (cmd == "?")
            {
                //Debug.WriteLine(response);
            }
            else
            {
                // Handle other commands if necessary
            }

        }

        List<string> ConvertToList(string rawInput)
        {
            // Split by newlines, remove empty entries, and trim any remaining whitespace
            List<string> stageList = rawInput
                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(stage => stage.Trim())
                .ToList();
            return stageList;
        }
        private void AppendOutput(string text)
        {
            Debug.WriteLine(text + "\n");
        }

        protected override void OnClosed(EventArgs e)
        {
            client.Close();
            base.OnClosed(e);
        }
    }
}