using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kSATxtCmdNETSDk;
using log4net;
using Sliders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace SliderLauncher
{
    public partial class TcpClientViewModel : ObservableObject
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private readonly DispatcherTimer _timer = new();
        private readonly kSATxtCmdClient cmdClient = new();
        CommandClientHandler client = new CommandClientHandler();
        private bool _isRunning = false;
        private SliderControlViewModel vm;
        private int _stageUpdateIndex = 0;
        bool connected = false;
        private TcpClient? _client;
        private NetworkStream? _stream;

        public SliderControlViewModel SliderViewModel { get; } = new();

        [ObservableProperty]
        private string ipAddress = "127.0.0.1";

        [ObservableProperty]
        private int port = 49215;

        [ObservableProperty]
        private string output = "";

        [ObservableProperty]
        private string inputMessage = "";

        [ObservableProperty]
        private bool autoLaunch = false;


        partial void OnAutoLaunchChanged(bool value)
        {
            if (value)
            {
                // Trigger the command when AutoLaunch is checked
                ConnectCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            Connect();

            //For streaming support instead of command client
            //try
            //{
            //    _client = new TcpClient();
            //    await _client.ConnectAsync(IpAddress, Port);
            //    //_stream = _client.GetStream();
            //    Output += $"Connected to {IpAddress}:{Port}\n";
            //    Debug.WriteLine(Output);
            //}
            //catch (Exception ex)
            //{
            //    Output += $"Connection failed: {ex.Message}\n";
            //}
            //IPAddressTextBox.Text = Properties.Settings.Default.IPAddress;
            //PortTextBox.Text = Properties.Settings.Default.Port.ToString();
            //if (int.TryParse(PortTextBox.Text, out int parsedPort))
            //{
            //    Port = parsedPort;
            //}
            //else
            //{
            //    AppendOutput("Invalid port number.");
            //    return;
            //}

            vm = new SliderControlViewModel();
            //sliderControl.DataContext = vm;
            if (!vm.Dummy)
            {
                //if(AutoLaunch)
                //ClientConnect(); //Auto connect on startup
                vm._stepSize = 2000 / (36 / 0.05); // update every 50ms
                _timer.Interval = TimeSpan.FromMilliseconds(200);
                _timer.Tick += (s, e) => RunCommands();
                _timer.Start();
            }
        }

        [RelayCommand]
        private async Task SendAsync()
        {
            if (_stream == null || !_stream.CanWrite)
            {
                Output += "Stream not available.\n";
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(InputMessage);
                await _stream.WriteAsync(data, 0, data.Length);
                Output += $"Sent: {InputMessage}\n";
            }
            catch (Exception ex)
            {
                Output += $"Send failed: {ex.Message}\n";
            }
        }

        [RelayCommand]
        private async Task ReceiveAsync()
        {
            if (_stream == null || !_stream.CanRead)
            {
                Output += "Stream not available.\n";
                return;
            }

            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Output += $"Received: {received}\n";
            }
            catch (Exception ex)
            {
                Output += $"Receive failed: {ex.Message}\n";
            }
        }

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

        private void Connect()
        {
            if (!connected) ClientConnect();
        }
        private void ClientConnect()
        {

            //if (int.TryParse(PortTextBox.Text, out int parsedPort))
            //{
            //    Port = parsedPort;
            //    Properties.Settings.Default.Port = Port;
            //}
            //else
            //{
            //    AppendOutput("Invalid port number.");
            //    return;
            //}
            Properties.Settings.Default.AutoLaunch = AutoLaunch;
            Properties.Settings.Default.Save(); // Save the port setting
            // bool connected = client.Connect("localhost", 49215, out var response, out var ret);
            //bool connected = client.Connect("192.168.3.10", 49215, out var response, out var ret);
            connected = client.Connect(IpAddress, Port, out var response, out var ret);
            Debug.WriteLine(connected ? $"Connected.\nResponse: {response}\nRet: {ret}" : response);
            Output += $"Connected to {IpAddress}:{Port}\n";
            Debug.WriteLine(Output);
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

        public void LoadAllSettings()
        {
            var settings = Properties.Settings.Default;
            var viewModelProps = this.GetType().GetProperties();
            var settingsProps = settings.GetType().GetProperties();

            foreach (var vmProp in viewModelProps)
            {
                if (!vmProp.CanWrite) continue;

                var match = settingsProps.FirstOrDefault(sp => sp.Name == vmProp.Name && sp.CanRead && sp.PropertyType == vmProp.PropertyType);
                if (match != null)
                {
                    var value = match.GetValue(settings);
                    vmProp.SetValue(this, value);
                }
            }
        }
        public void SaveAllSettings()
        {
            var settings = Properties.Settings.Default;
            var viewModelProps = this.GetType().GetProperties();
            var settingsProps = settings.GetType().GetProperties();

            foreach (var vmProp in viewModelProps)
            {
                if (!vmProp.CanRead) continue;

                var match = settingsProps.FirstOrDefault(sp => sp.Name == vmProp.Name && sp.CanWrite && sp.PropertyType == vmProp.PropertyType);
                if (match != null)
                {
                    var value = vmProp.GetValue(this);
                    match.SetValue(settings, value);
                }
            }

            settings.Save();
        }

        public bool OnClosing()
        {
            _timer?.Stop();
            // Stop any custom client logic (SDK or polling)
            // Disconnect client gracefully
            try
            {
                client?.Close(); // ensure this closes sockets or background threads
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while disconnecting client: {ex.Message}");
            }
            

            // Stop any manual TcpClient/Stream
            try
            {
                _stream?.Close();
                _stream?.Dispose();
                _stream = null;

                _client?.Close();
                _client?.Dispose();
                _client = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during TCP shutdown: {ex.Message}");
            }

            // Forward to child view model for graceful shutdown
            return SliderViewModel?.OnClosing() ?? true;
        }
    }

}
