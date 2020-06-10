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
