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
    class NewServer
    {
        private Socket m_ServerSocket;
        private List<Socket> m_ClientSocket;
        private byte[] szData;

        public void InitServer()
        {
            m_ClientSocket = new List<Socket>();

            try
            {
                m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 11200);
                m_ServerSocket.Bind(endPoint);
                m_ServerSocket.Listen(10);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            m_ServerSocket.AcceptAsync(args);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = e.AcceptSocket;
            m_ClientSocket.Add(ClientSocket);

            if (m_ClientSocket != null)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                szData = new byte[1024];
                args.SetBuffer(szData, 0, 1024);
                args.UserToken = m_ClientSocket;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                ClientSocket.ReceiveAsync(args);
            }

            e.AcceptSocket = null;
            m_ServerSocket.AcceptAsync(e);
        }
        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = (Socket)sender;
            if (ClientSocket.Connected && e.BytesTransferred > 0)
            {
                byte[] szData = e.Buffer;    // 데이터 수신
                string sData = Encoding.UTF8.GetString(szData);

                Console.WriteLine(sData.Replace("\0", "").Trim());

                for (int i = 0; i < szData.Length; i++)
                {
                    szData[i] = 0;
                }
                e.SetBuffer(szData, 0, 1024);
                ClientSocket.ReceiveAsync(e);
            }
            else
            {
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                m_ClientSocket.Remove(ClientSocket);
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            NewServer svr = new NewServer();
            svr.InitServer();

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
        
        private static void clientFunc(object obj)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                EndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 11200);

                socket.Connect(serverEP);

                byte[] buf = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                socket.Send(buf);

                socket.Close();
            }

        }
    }
}
