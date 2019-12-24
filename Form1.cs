using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace SimulatorServer
{
    public partial class Form1 : Form
    {
        public string serverIP = "127.0.0.1";
        public int port = 3000;
        TcpListener listener;
        ArrayList sockets;

        public Form1()
        {
            InitializeComponent();
            InitServer();
        }

        void ExecuteCommand(string command)
        {
            System.Diagnostics.Process cmd = new System.Diagnostics.Process();
            string fileName = "/bin/bash";
            cmd.StartInfo.FileName = fileName;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        private void InitServer()
        {
            sockets = new ArrayList();
            IPAddress address = IPAddress.Parse(serverIP);

            listener = new TcpListener(address, port);

            listener.Start();

            Console.WriteLine("Server started on " + listener.LocalEndpoint);

            Thread serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.Start();
        }

        private void StartServer()
        {
            Console.WriteLine("Waiting for a connection...");

            while (true)
            {
                try
                {
                    Socket socket = listener.AcceptSocket();
                    Console.WriteLine("Connection received from " + socket.RemoteEndPoint);
                    sockets.Add(socket);
                    Thread clientService = new Thread(new ThreadStart(() => ServiceClient(socket)));
                    clientService.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void ServiceClient(Socket socket)
        {
            bool keepAlive = true;
            var stream = new NetworkStream(socket);
            var reader = new StreamReader(stream);

            while (keepAlive)
            {
                string str = reader.ReadLine();
                if (str.ToUpper() == "DONE") break;
            }

            sockets.Remove(socket);
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            foreach (Socket socket in sockets)
            {
                var stream = new NetworkStream(socket);
                var writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                writer.WriteLine("START");
            }
        }

        private void LaunchBtn_Click(object sender, EventArgs e)
        {
            string name1 = textBox1.Text;
            string host1 = textBox2.Text;
            string name2 = textBox3.Text;
            string host2 = textBox4.Text;

            if (name1 != "")
            {
                ExecuteCommand(String.Format("/mnt/01D572737FF2C600/cds-de-thi-2019/de1/simulator.x86_64 -name {0} -host {1} &", name1, host1));
            }
            if (name2 != "")
            {
                ExecuteCommand(String.Format("/mnt/01D572737FF2C600/cds-de-thi-2019/de1/simulator.x86_64 -name {0} -host {1} &", name2, host2));
            }
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            ExecuteCommand("killall simulator.x86_64");
        }
    }
}
