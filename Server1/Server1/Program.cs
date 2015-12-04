using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server1
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread serverThread = new Thread(serverFunc);
            serverThread.IsBackground = true;
            serverThread.Start();

            Thread.Sleep(500);

            int clientCount = 10;
            for (int i = 0; i < clientCount; i++)
            {
                Thread clientThread = new Thread(clientFunc);
                clientThread.IsBackground = true;
                clientThread.Start();
            }

            Console.WriteLine("종료하려면 아무 키나 누르세요.");
            Console.ReadLine();

        }

        private static void serverFunc(object obj)
        {
            using (Socket srvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 11200);
                srvSocket.Bind(endPoint);
                srvSocket.Listen(10);

                while (true)
                {
                    Socket clntSocket = srvSocket.Accept();

                    ThreadPool.QueueUserWorkItem((WaitCallback)clientSocketProcess, clntSocket);
                }

            }
        }

        private static void clientSocketProcess(object state)
        {
            Socket clntSocket = state as Socket;

            byte[] recvBytes = new byte[1024];

            int nRecv = clntSocket.Receive(recvBytes);
            string txt = Encoding.UTF8.GetString(recvBytes, 0, nRecv);

            byte[] sendBytes = Encoding.UTF8.GetBytes("Hello: " + txt);
            clntSocket.Send(sendBytes);
            clntSocket.Close();
        }

        private static void clientFunc(object obj)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                EndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 11200);

                socket.Connect(serverEP);

                byte[] buf = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                socket.Send(buf);

                byte[] recvBytes = new byte[1024];
                int nRecv = socket.Receive(recvBytes);
                string txt = Encoding.UTF8.GetString(recvBytes, 0, nRecv);

                Console.WriteLine(txt);

                socket.Close();
                Console.WriteLine("TCP Client socket: Closed");

            }

        }
    }
}
