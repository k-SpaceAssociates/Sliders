using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kSATxtCmdNETSDk;
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

namespace ClientTestApp
{
    public partial class TcpClientViewModel : ObservableObject
    {
        private readonly kSATxtCmdClient cmdClient = new();
        CommandClientHandler client = new CommandClientHandler();
        private bool _isRunning = false;
        private int _stageUpdateIndex = 0;
        bool connected = false;
        private TcpClient? _client;
        private NetworkStream? _stream;

        [ObservableProperty]
        private string iPAddress = "127.0.0.1";

        [ObservableProperty]
        private int port = 49215;

        [ObservableProperty]
        private string output = "";

        [ObservableProperty]
        private string input = "";

        [ObservableProperty]
        private bool autoLaunch = false;


        //partial void OnAutoLaunchChanged(bool value)
        //{
        //    if (value)
        //    {
        //        // Trigger the command when AutoLaunch is checked
        //        ConnectCommand.Execute(null);
        //    }
        //}

        [RelayCommand]
        private async Task ConnectAsync()
        {
            Connect();

            //For streaming support instead of command client
            //try
            //{
            //    _client = new TcpClient();
            //    await _client.ConnectAsync(IPAddress, Port);
            //    //_stream = _client.GetStream();
            //    Output += $"Connected to {IPAddress}:{Port}\n";
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

          }



        [RelayCommand]
        private async Task SendAsync()
        {

            Send();
            //if (_stream == null || !_stream.CanWrite)
            //{
            //    Output += "Stream not available.\n";
            //    return;
            //}

            //try
            //{
            //    byte[] data = Encoding.UTF8.GetBytes(Input);
            //    await _stream.WriteAsync(data, 0, data.Length);
            //    Output += $"Sent: {Input}\n";
            //}
            //catch (Exception ex)
            //{
            //    Output += $"Send failed: {ex.Message}\n";
            //}
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

        private void Send()
        {
           client.Send(Input, out var response, out var ret);
            Debug.WriteLine($"Ret: {ret}\nCommand: {Input}\nResponse:{response}\n");
            Output += $"Sent: {Input}\nResponse: {response}\n";
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
            //Properties.Settings.Default.AutoLaunch = AutoLaunch;
            Properties.Settings.Default.Save(); // Save the port setting
            // bool connected = client.Connect("localhost", 49215, out var response, out var ret);
            //bool connected = client.Connect("192.168.3.10", 49215, out var response, out var ret);
            connected = client.Connect(IPAddress, Port, out var response, out var ret);
            Debug.WriteLine(connected ? $"Connected.\nResponse: {response}\nRet: {ret}" : response);
            Output += $"Connected to {IPAddress}:{Port}\n";
            Debug.WriteLine(Output);
        }

        //private void Send(string command)
        //{
        //    bool sent = client.Send(command, out var response, out var ret);
        //    Debug.WriteLine($"Ret: {ret}\nCommand: {command}\nResponse:{response}\n");
        //}

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
            return true; // allow close

        }


    }
}
