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


        public void ResponseError(int code, String status, String error = null)
        {
            Header.Code = code;
            Header.Status = status;

            Body = Encoding.UTF8.GetBytes(error == null ? String.Format("<html><head><title>{0}</title></head><body><center>{0}</center></body></html>", code + " " + status) : error);
        }
    }
}
