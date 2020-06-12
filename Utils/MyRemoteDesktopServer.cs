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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsManager.Utils
{
    public class MyRemoteDesktopServer:IDisposable
    {
        SocketsLayerWithQueue SocketsLayerWithQueue;
        public event EventHandler<RemoteServerAction> ActionRequestReceived;
        public event EventHandler<(string message, Exception e)> SocketExceptionReceived;
        public MyRemoteDesktopServer(int sendPort, int receivePort)
        {
            SocketsLayerWithQueue = new SocketsLayerWithQueue(sendPort, receivePort, callback: DataReceived);
            SocketsLayerWithQueue.ExceptionCatched += (o, e) => this.SocketExceptionReceived?.Invoke(o, e);
        }

        public void DataReceived(byte[] data)
        {
            RemotePacket packet = Deserialize<RemotePacket>(data);
            if(packet != default)
            {
                ActionRequestReceived?.Invoke(this, packet.Action);
                switch (packet.Action)
                {
                    case RemoteServerAction.GetMousePosition:
                        
                        Send(Serialize(new RemotePacket
                        {
                            Action = RemoteServerAction.GetMousePosition,
                            Data = Serialize(Cursor.Position),
                            DataType = typeof(Point)
                        }));
                        break;

                    case RemoteServerAction.GetScreen:
                        
                        Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        Graphics.FromImage(bitmap).CopyFromScreen(new Point(), new Point(), bitmap.Size);
                        Send(Serialize(new RemotePacket { 
                            Action = RemoteServerAction.GetScreen,
                            Data = Serialize(bitmap),
                            DataType = typeof(Bitmap)
                        }));
                        break;

                    case RemoteServerAction.SendKeys:
                        if(packet.DataType == typeof(Keys))
                        {
                            Keys keys = Deserialize<Keys>(packet.Data);
                            
                        }
                        break;

                    case RemoteServerAction.SetMousePosition:
                        if (packet.DataType == typeof(Point))
                        {
                            Cursor.Position = Deserialize<Point>(packet.Data);
                        }
                        break;
                }
            }
        }

        public void Send(byte[] data)
        {
            SocketsLayerWithQueue.EnqueueSend(data);
        }
        public Point GetMousePosition()
        {
            return Cursor.Position;
        }

        public void Dispose()
        {
            SocketsLayerWithQueue?.Dispose();
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
    }

    [Serializable]
    public class RemotePacket
    {
        public RemoteServerAction Action { get; set; }
        public Type DataType { get; set; }
        public byte[] Data{get;set;}
    }

    [Serializable]
    public enum RemoteServerAction
    {
        GetScreen,
        GetMousePosition,
        SetMousePosition,
        SendKeys
    }

}
