using System;
using System.Net;

namespace Atlantis.Consul
{
    public class ConsulServiceOptions
    {
        public ConsulServiceOptions()
        {
            ID=Guid.NewGuid().ToString();
        }
        
        public string ServiceName{get;set;}

        public string Address{get;set;}

        public int Port{get;set;}

        public string[] Tags{get;set;}

        /// <summary>
        /// TTL check time (util: ms)  
        /// </summary>
        public int TTL{get;set;}

        /// <summary>
        /// Timeout for request(util: ms)  
        /// </summary>
        public int Timeout{get;set;}

        internal string ID{get;}

        public string ServiceID=>$"{ServiceName}-{ID}";
    }
}
