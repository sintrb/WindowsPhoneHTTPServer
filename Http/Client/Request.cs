using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http.Client
{
    public class Request
    {
        public RequestHeader Header { get; set; }
        public byte[] Body { get; set; }

        public System.Net.Sockets.Socket Client { get; set; }
    }
}
