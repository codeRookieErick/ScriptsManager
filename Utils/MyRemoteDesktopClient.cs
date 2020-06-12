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
            catch (Exception)
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
