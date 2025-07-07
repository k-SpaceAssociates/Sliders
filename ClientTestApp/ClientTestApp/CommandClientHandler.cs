using kSATxtCmdNETSDk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTestApp
{
    public class CommandClientHandler
    {
        private readonly kSATxtCmdClient cmdClient = new();

        public bool Connect(string host, int port, out string response, out int ret)
        {
            if (cmdClient.ConnectToServer(host, port))
            {
                response = "";

                string outstring = cmdClient.GetConnectResponse(ref response);
                if (outstring == "READY")
                    ret = 0; // Assuming 0 means success
                else ret = -1;
                return true;
            }
            response = "Failed to connect to server.";
            ret = -1;
            return false;
        }

        public bool Send(string command, out string response, out int ret)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                response = "";
                string outstring = cmdClient.IssueCommand(command, ref response);
                ret = 0; // Assuming 0 means success
                return true;
            }
            response = "Empty command.";
            ret = -1;
            return false;
        }

        public void Close()
        {
            cmdClient.Close();
        }
    }

}
