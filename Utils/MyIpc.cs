using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ScriptsManager.Utils
{
    public class MyIpc : IDisposable
    {
        static int startPort = 40010;
        static List<int> usedPorts = new List<int> { startPort };
        public static int GetFreePort()
        {
            int port = usedPorts.Max() + 1;
            usedPorts.Add(port);
            return port;
        }


        public static MyIpc Create(Action<string> callback)
        {
            return new MyIpc(
                    GetFreePort(),
                    GetFreePort(),
                    callback
                );
        }
        public const int MAIN_THREAD_WAIT_MILLISECONDS = 100;
        public const int RECEIVE_BUFFER_LENGTH = 10240;
        public Action<string> ReceiveCallback { get; private set; }
        public int ClientPort { get; private set; }
        public int ServerPort { get; private set; }
        Socket receiveSocket, sendSocket;
        Thread MainThread { get; set; } = null;
        public bool Running { get; private set; } = false;
        public MyIpc(int serverPort, int clientPort, Action<string> receiveCallback = null)
        {
            ReceiveCallback = receiveCallback ?? ((d) => { });
            this.ClientPort = clientPort;
            this.ServerPort = serverPort;
            Start();
        }


        private void Start()
        {
            if (Running) return;
            Running = true;
            MainThread = new Thread(MainEventLoop);
            MainThread.Start();
        }
        /*
        class MyAsyncWaste : IAsyncResult
        {
            public bool IsCompleted => true;

            Mutex mutex = new Mutex();
            public WaitHandle AsyncWaitHandle => mutex;

            public object AsyncState => null;

            public bool CompletedSynchronously => true;
        }*/
        void MainEventLoop()
        {
            try
            {
                receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                receiveSocket.Bind(new IPEndPoint(IPAddress.Loopback, ClientPort));
                receiveSocket.Listen(5);
                byte[] buffer = new byte[RECEIVE_BUFFER_LENGTH];
                try
                {
                    AsyncCallback asyncCallback = null;

                    asyncCallback = (s) => {
                        Socket socket = s.AsyncState as Socket;
                        try
                        {
                            Socket handler = socket.EndAccept(s);
                            int readLength = handler.Receive(buffer);
                            ReceiveCallback(Encoding.ASCII.GetString(buffer.Take(readLength).ToArray()));
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            socket.BeginAccept(asyncCallback, socket);
                        }catch(Exception e)
                        {

                        } 
                    };

                    receiveSocket.BeginAccept(asyncCallback, receiveSocket);
                }
                finally
                {
                }
            }
            catch(ThreadAbortException e)
            {

            }
            catch (Exception e)
            {

            }
        }

        public bool Send(string data)
        {
            try
            {
                sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sendSocket.Connect(new IPEndPoint(IPAddress.Loopback, ServerPort));
                sendSocket.Send(
                        Encoding.ASCII.GetBytes(data)
                    );
                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();
            }catch(Exception e)
            {

            }
            return true;
        }

        public void Kill()
        {
            if (!Running) return;
            Running = false;
            usedPorts.Remove(this.ServerPort);
            usedPorts.Remove(this.ClientPort);
            try
            {
                //receiveSocket.EndAccept(new MyAsyncWaste());
                receiveSocket.Close();
                MainThread.Join();
                MainThread.Abort();
            }
            catch(Exception e)
            {

            }
        }

        public void Dispose()
        {
            Kill();
        }
    }
}
