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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class SocketsLayerWithQueue : SocketsLayer, IDisposable
    {
        LimitedQueue<byte[]> queue;
        public bool Running { get; private set; } = false;
        Thread Thread;
        public const int SEND_WAIT = 20;
        public SocketsLayerWithQueue(
            int sendPort, 
            int receivePort,
            int messagesLength = 100,
            string remoteHostNameOrIp = null, 
            Action<byte[]> callback = null, 
            BufferFechMode FetchMode = BufferFechMode.Split) : 
            base(sendPort, receivePort, remoteHostNameOrIp, callback, FetchMode)
        {
            queue = new LimitedQueue<byte[]>();
            Thread = new Thread(SendEventLoop);
            Thread.Start();
        }

        public void EnqueueSend(byte[] data)
        {
            lock (queue)
            {
                queue.Enqueue(data);
            }
        }

        void SendEventLoop()
        {
            if (Running) return;
            Running = true;
            while (Running)
            {
                lock (queue)
                {
                    if (queue.Count > 0) Send(queue.Dequeue());
                }
                Thread.Sleep(SEND_WAIT);
            }
        }

        public override void Dispose()
        {
            Running = false;
            base.Dispose();
        }
    }
}
