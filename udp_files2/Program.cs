using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace udp_files
{
    class Program
    {
        static IPAddress remote_address;
        const int remotePort = 8001;
        const int localPort = 8002;
        static string username;
        static string way;
        static void Main(string[] args)
        {
            username = "USR1";
            Console.WriteLine("Input path for saved files:");
            way = Console.ReadLine();
            way = way + '\\';
            remote_address = IPAddress.Parse("235.5.5.11");
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();
            SendMessage();
        }

        private static void SendMessage()
        {
            UdpClient sender = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(remote_address, remotePort);
            try
            {
                while (true)
                {
                    Console.WriteLine("Input full path for sending file: ");
                    string message = Console.ReadLine();
                    string tempway = Path.GetFileName(message);
                    byte[] array = Encoding.Unicode.GetBytes(tempway);
                    sender.Send(array, array.Length,endPoint);
                    Thread.Sleep(1000);
                    using (FileStream fstream = File.OpenRead(@message))
                    {
                        byte[] data = new byte[fstream.Length];
                        fstream.Read(data, 0, data.Length);
                        sender.Send(data, data.Length, endPoint);
                    }
                        //message = string.Format("{0} : {1}", username, message);
                        //byte[] data = Encoding.Unicode.GetBytes(message);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(localPort);
            receiver.JoinMulticastGroup(remote_address, 50);
            IPEndPoint remoteIP = null;
            string path="";
            string localAddress = LocalIpAddress();
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIP);
                    //if (remoteIP.Address.ToString().Equals(localAddress))
                        //continue;
                    string message = Encoding.Unicode.GetString(data);
                    path = way + '\\' + message;
                    using (FileStream fstream = new FileStream(@path, FileMode.Create))
                    {
                        byte[] arr = receiver.Receive(ref remoteIP);
                        //string text = Encoding.ASCII.GetString(arr);
                        fstream.Write(arr,0,arr.Length);
                    }
                    Console.WriteLine("File receive.");
                    //Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static string LocalIpAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;

        }
    }


}
