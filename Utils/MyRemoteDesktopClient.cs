using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class MyRemoteDesktopClient:IDisposable
    {
        public event EventHandler<Bitmap> DesktopReceived;
        public event EventHandler<Point> MousePositionChanged;

        SocketsLayerWithQueue SocketsLayerWithQueue;
        public MyRemoteDesktopClient(int sendPort, int receivePort, string hostname)
        {
            SocketsLayerWithQueue = new SocketsLayerWithQueue(sendPort, receivePort, remoteHostNameOrIp:hostname, callback: DataReceived);
        }

        public void RequestScreen()
        {
            Send(Serialize(new RemotePacket
            {
                Action = RemoteServerAction.GetScreen,
                Data = Serialize(new byte[] { }),
                DataType = typeof(byte[])
            }));
        }

        public void SetMousePosition(Point point)
        {
            Send(Serialize(new RemotePacket { 
                Action = RemoteServerAction.SetMousePosition,
                Data = Serialize(point),
                DataType =typeof(Point)
            }));
        }

        public void SendKeys(Keys keys)
        {
            Send(Serialize(new RemotePacket
            {
                Action = RemoteServerAction.SendKeys,
                Data = Serialize(keys),
                DataType = typeof(Keys)
            }));
        }

        void Send(byte[] data)
        {
            SocketsLayerWithQueue?.EnqueueSend(data);
        }

        void DataReceived(byte[] data)
        {
            RemotePacket packet = Deserialize<RemotePacket>(data);
            if(packet != null)
            {
                switch (packet.Action)
                {
                    case RemoteServerAction.GetScreen:
                        if(packet.DataType == typeof(Bitmap) && DesktopReceived != null)
                        {
                            DesktopReceived(this, this.Deserialize<Bitmap>(packet.Data));
                        }
                        break;
                }
            }
        }



        public T Deserialize<T>(byte[] data)
        {
            T packet = default;
            try
            {
                packet = (T)(new BinaryFormatter().Deserialize(new MemoryStream(data)));
            }
            catch (Exception e)
            {

            }
            return packet;
        }

        public byte[] Serialize<T>(T graph)
        {
            MemoryStream memoryStream = new MemoryStream();
            new BinaryFormatter().Serialize(memoryStream, graph);
            return memoryStream.ToArray();
        }

        public void Dispose()
        {
            SocketsLayerWithQueue?.Dispose();
        }
    }
}
