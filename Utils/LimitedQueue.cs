using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class LimitedQueue<T>:Queue<T>
    {
        int maximun;
        public LimitedQueue(int maximun = 100)
        {
            this.maximun = maximun;
        }
        public new void  Enqueue(T element)
        {
            while (Count > maximun) Dequeue();
            base.Enqueue(element);
        }
    }
}
