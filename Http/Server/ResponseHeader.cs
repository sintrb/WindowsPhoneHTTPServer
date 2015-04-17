using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http.Server
{
    public class ResponseHeader : Header
    {
        public int Code { get; set; }
        public String Status { get; set; }
        public String Protocol { get; set; }
        public ResponseHeader() { }

        override public String ToHttpHeader()
        {
            FirstLine = Protocol + " " + Code + " " + Status;
            return base.ToHttpHeader();
        }
    }
}
