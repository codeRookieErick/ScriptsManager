using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class Parser
    {
        public object Parse(string data)
        {

            return Newtonsoft.Json.JsonConvert.DeserializeObject(data);
        }
    }
}
