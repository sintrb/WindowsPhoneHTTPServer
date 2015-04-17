using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http.Server
{
    public class Response
    {
        public ResponseHeader Header { get; set; }
        public byte[] Body { get; set; }
    }
}
