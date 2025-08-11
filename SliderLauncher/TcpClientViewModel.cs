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
using System.Windows;
using System.Windows.Threading;


namespace SliderLauncher
{
    public partial class TcpClientViewModel : ObservableObject
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private readonly DispatcherTimer _timer = new();
        private readonly kSATxtCmdClient cmdClient = new();
        private readonly CommandClientHandler client = new();
        private readonly SliderControlViewModel _sliderVM; // injected UI VM

        private bool _isRunning = false;
        private int _stageUpdateIndex = 0;
        private int _stageUpdateIndex2 = 0;
        private bool connected = false;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private bool verboseOutput = false; // Set to true for detailed debug output

        [ObservableProperty]
        private string ipAddress = "127.0.0.1";

        [ObservableProperty]
        private int port = 49215;

        [ObservableProperty]
        private int streamport = 8000;

        [ObservableProperty]
        private string output = "";

        [ObservableProperty]
        private string inputMessage = "";

        [ObservableProperty]
        private bool autoLaunch = false;

        [ObservableProperty]
        private bool useStreaming = false; // ✅ Switch between streaming and polling

        // ✅ Constructor now takes SliderControlViewModel from MainWindow
        public TcpClientViewModel(SliderControlViewModel sliderVM)
        {
            _sliderVM = sliderVM;
        }
        partial void OnAutoLaunchChanged(bool value)
        {
            if (value)
            {
                // Trigger the command when AutoLaunch is checked
                ConnectCommand.Execute(null);
            }
        }

        private CancellationTokenSource? _cts;

        [RelayCommand]
        public async Task ConnectAsync()
        {
            Connect();

            ////For streaming support instead of command client
            //try
            //{
            //    _client = new TcpClient();
            //    await _client.ConnectAsync(IpAddress, Port);
            //    _stream = _client.GetStream();

            //    Output += $"Connected to {IpAddress}:{Port}\n";
            //    Debug.WriteLine(Output);

            //    _ = Task.Run(() => ReadLoopAsync()); // fire and forget
            //}
            //catch (Exception ex)
            //{
            //    Output += $"Connection failed: {ex.Message}\n";
            //}

            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += (s, e) =>
            {
                RunCommands();
                _sliderVM.UpdateSlider(); // push updates to UI-bound VM
            };
            _timer.Start();
        }

        //////////////////////////////////////////////////////////////////////////////////
        // Streaming support section
        //////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Stream reading loop to handle incoming messages asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task ReadLoopAsync(CancellationToken token)
        {
            var buffer = new byte[1024];
            try
            {
                while (_client?.Connected == true && !token.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // connection closed

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessIncomingMessage(message);
                    Debug.WriteLine(message + "\n");
                }
            }
            catch (OperationCanceledException)
            {
                Output += "Stream reading canceled.\n";
            }
            catch (Exception ex)
            {
                Output += $"Stream read error: {ex.Message}\n";
            }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _client?.Close();
            _timer.Stop();
        }
        private void ProcessIncomingMessage(string message)
        {
            // Parse and update UI-bound VM
            Application.Current.Dispatcher.Invoke(() =>
            {
                _sliderVM.CurrentValue = ParseSliderValue(message);
            });
        }

        private double ParseSliderValue(string message)
        {
            // TODO: parse incoming protocol to extract value
            return double.TryParse(message, out var value) ? value : 0;
        }

        //////////End Streaming support section///////////////////////////////////////////

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
                if(verboseOutput) Output += $"Received: {received}\n";
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
                foreach (var command in _sliderVM.CommandsToRun)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        if (command == "position")
                        {
                            if (_sliderVM.StageList != null)
                            {
                                foreach (string stage in _sliderVM.StageList) //There will be no stages if connection is lost
                                {
                                    Send(command + " " + stage);
                                }
                            }
                        }
                        else if (command == "fakeHpos")
                        {
                           if(_sliderVM.FakeStageHoriz) Send(command + " 1"); //enable stage dummy data for horizontal position
                           else Send(command + " 0"); //disable stage dummy data for horizontal position
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
            if (verboseOutput) Debug.WriteLine($"Ret: {ret}\nCommand: {command}\nResponse:{response}\n");
            AssignResults(command, response);
        }

        void AssignResults(string cmd, string response)
        {
            if (response == "") { Output=$"Error: No response received\n"; Debug.WriteLine(Output); }
            if (cmd == "list") //Assume list gets Stages available
            {
                var stages = ConvertToList(response);
                _sliderVM.StageList.Clear();
                foreach (var stage in stages)
                    _sliderVM.StageList.Add(stage);
            }
            else if (cmd.StartsWith("position", StringComparison.OrdinalIgnoreCase))
            {
                var stagePosition = cmd.Substring("position".Length).Trim();
                //if(_sliderVM.StagePositions.Count == 4) _sliderVM.StagePositions.Clear(); //This will clear current info. and start getting next 4

                var lines = response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var newValue = lines[0];

                    if (_sliderVM.StagePositions.Count < 4)
                    {
                        _sliderVM.StagePositions.Add(newValue); // Add until you have 4 items
                        //SliderVM.StagePositions.Add(newValue);
                    }
                    else
                    {
                        _sliderVM.StagePositions[_stageUpdateIndex] = newValue; // Overwrite existing item
                    }

                    _stageUpdateIndex = (_stageUpdateIndex + 1) % 4; // Wrap index from 0 to 3
                }
                if (_sliderVM.StagePositions.Count == 4)
                    _sliderVM.UpdateSlider();
            }
            else if (cmd == "direction")
            {
                var lines = response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                    _sliderVM.Direction = lines[0];
            }

            else if (cmd == "follow")
            {
                if (response.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _sliderVM.Follow = false;
                }
                else if (response.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _sliderVM.Follow = true;
                }
                else _sliderVM.Follow = false;
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
            //return _sliderVM?.OnClosing() ?? true;
            return true;
        }
    }

}
