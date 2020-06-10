using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class BackgroundTask:IDisposable
    {
        public const int MIN_WAIT_INTERVAL = 100;
        public const int MAX_WAIT_INTERVAL = 10000;

        static int defaultWaitInterval;
        public static int DefaultWaitInterval
        {
            get => defaultWaitInterval; 
            set
            {
                defaultWaitInterval = Math.Min(Math.Max(value, MIN_WAIT_INTERVAL), MAX_WAIT_INTERVAL);
            }
        }

        static BackgroundTask()
        {
            DefaultWaitInterval = MIN_WAIT_INTERVAL;
        }

        int waitMilliseconds;
        public int WaitMilliseconds
        {
            get => waitMilliseconds;
            set
            {
                this.waitMilliseconds =
                    value != default ? Math.Min(Math.Max(value, MIN_WAIT_INTERVAL), MAX_WAIT_INTERVAL) : DefaultWaitInterval;
            }
        }

        public enum Mode
        {
            /// <summary>
            /// The minimun wait interval. Equal to <code>MIN_WAIT_INTERVAL</code>
            /// </summary>
            Interactive = MIN_WAIT_INTERVAL,
            /// <summary>
            /// One second.
            /// </summary>
            Moderate = 1000,
            /// <summary>
            /// The maximun wait interval. Equal to <code>MAX_WAIT_INTERVAL</code>
            /// </summary>
            Slept = MAX_WAIT_INTERVAL
        }


        Action action;
        Thread thread;
        public bool Running { get; private set; } = false;
        public BackgroundTask(Action action, int waitMilliseconds = default)
        {
            this.WaitMilliseconds = waitMilliseconds;
            this.action = action;
            this.thread = new Thread(MainEventLoop);
            this.thread.Start();
        }

        void MainEventLoop()
        {
            if (Running) return;
            Running = true;
            try
            {
                while (Running)
                {
                    action();
                    Thread.Sleep(WaitMilliseconds);
                }
            }
            finally
            {
                Running = false;
            }
        }

        public void SetMode(Mode mode)
        {
            this.WaitMilliseconds = (int)mode;
        }

        public void RequestStop()
        {
            this.Running = false;
        }

        public void Dispose()
        {
            RequestStop();
            thread.Join();
        }
    }
}
