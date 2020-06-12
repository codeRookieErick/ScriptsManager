    /*
    ScriptsManager, Administrador de scripts
    Copyright (C) 2020 Erick Mora

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    erickfernandomoraramirez@gmail.com
    erickmoradev@gmail.com
    https://dev.moradev.dev/myportfolio
    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class SocketsLayer : IDisposable
    {
        const int BUFFER_LENGTH = 10240;
        public event EventHandler<SocketPacket> PacketReceived;
        public event EventHandler<(string message, Exception exception)> ExceptionCatched;
        public enum BufferFechMode
        {
            /// <summary>
            /// Pases data splitted by BUFFER_LENGTH
            /// </summary>
            Split,
            /// <summary>
            /// Groups all data in one callback
            /// </summary>
            All
        }
        public int SendPort { get; private set; }
        public int ReceivePort { get; private set; }

        public IPAddress RemoteIpAddress { get; private set; }
        public Action<byte[]> Callback { get; private set; }
        Socket receiveSocket;
        public SocketsLayer(
            int sendPort,
            int receivePort,
            string remoteHostNameOrIp = null,
            Action<byte[]> callback = null, BufferFechMode FetchMode = BufferFechMode.Split)
        {
            this.SendPort = sendPort;
            this.ReceivePort = receivePort;
            this.Callback = callback ?? (d => { });

            if (!string.IsNullOrEmpty(remoteHostNameOrIp))
            {
                RemoteIpAddress = Dns.GetHostAddresses(remoteHostNameOrIp).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            }

            if (RemoteIpAddress == default)
            {
                RemoteIpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            }

            StartListenLoop();
        }

        void StartListenLoop()
        {
            receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiveSocket.Bind(new IPEndPoint(IPAddress.Any, ReceivePort));
            receiveSocket.Listen(10);
            AsyncCallback asyncCallback = null;
            List<byte> acumulator = new List<byte>();
            int bytesReaded = 0;
            asyncCallback = (r) =>
            {
                Socket socket = r.AsyncState as Socket;
                Socket handler = null;
                try
                {
                    handler = socket.EndAccept(r);
                    byte[] buffer;
                    int contentSize = 0;
                    while((bytesReaded = handler.Receive((buffer = Serialize(contentSize)))) > 0)
                    {

                        contentSize = Deserialize<int>(buffer);
                        buffer = new byte[contentSize];
                        bytesReaded = handler.Receive(buffer);
                        byte[] data = buffer.Take(bytesReaded).ToArray();
                        SocketPacket packet = null;
                        try
                        {
                            packet = Deserialize<SocketPacket>(data);
                            RemoteIpAddress = Dns
                                .GetHostAddresses(packet.SenderHostame)
                                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                            Callback(packet.Data);
                            PacketReceived?.Invoke(this, packet);
                        }
                        catch (Exception e)
                        {
                            byte[] waste = new byte[10240];
                            while ((bytesReaded = handler.Receive(waste)) > 0) ;
                            ExceptionCatched?.Invoke(this, ("On receive => deserialization", e));
                        }
                    }
                }
                finally
                {
                    try
                    {

                        handler?.Shutdown(SocketShutdown.Both);
                        handler?.Close();
                    }
                    catch (Exception e) 
                    {
                        ExceptionCatched?.Invoke(this, ("On receive => closing handler socket", e));
                    }
                    socket?.BeginAccept(asyncCallback, socket);
                }
            };

            receiveSocket.BeginAccept(asyncCallback, receiveSocket);
        }

        public Socket GetListener()
        {

            Socket result = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            result.Connect(new IPEndPoint(RemoteIpAddress, SendPort));
            return result;
        }
        Mutex mutex = new Mutex();
        public void Send(byte[] data)
        {
            new Thread(() =>
            {
                mutex.WaitOne();
                Socket sendSocket = null;
                bool sockedHasConnected = false;
                try
                {
                    sendSocket = GetListener();
                    sockedHasConnected = true;
                    byte[] rawData = Serialize(
                        new SocketPacket()
                        {
                            Data = data,
                            SenderHostame = Dns.GetHostEntry(Dns.GetHostName())
                                .AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString(),
                            MetaData = new Dictionary<string, string>()
                        }
                    );
                    byte[] dataLength = Serialize(rawData.Length);
                    byte[] composedBlob = new byte[dataLength.Length + rawData.Length];
                    dataLength.CopyTo(composedBlob, 0);
                    rawData.CopyTo(composedBlob, dataLength.Length);
                    sendSocket.Send(composedBlob);
                }
                catch (Exception e)
                {
                    ExceptionCatched?.Invoke(this, ("On send => building and sending blob", e));
                }
                finally
                {
                    try
                    {
                        if (sockedHasConnected)
                        {
                            sendSocket?.Shutdown(SocketShutdown.Both);
                            sendSocket?.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionCatched?.Invoke(this, ("On send => building and sending blob", e));
                    }
                    finally
                    {

                    }
                    mutex.ReleaseMutex();
                }
            }).Start();
        }

        [Serializable]
        public class SocketPacket
        {
            public string SenderHostame = "";
            public Dictionary<string, string> MetaData = new Dictionary<string, string>();
            public byte[] Data = new byte[1024];


        }
        public static T Deserialize<T>(byte[] raw)
        {
            return (T)new BinaryFormatter().Deserialize(new MemoryStream(raw));
        }

        public static byte[] Serialize<T>(T graph)
        {
            MemoryStream memoryStream = new MemoryStream();
            new BinaryFormatter().Serialize(memoryStream, graph);
            return memoryStream.ToArray();
        }


        public virtual void Dispose()
        {
            try
            {
                receiveSocket?.Shutdown(SocketShutdown.Both);
                receiveSocket?.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
