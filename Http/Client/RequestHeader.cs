using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
namespace Sin.Http.Client
{
    public class RequestHeader:Header
    {
        public String Path { get; set; }
        public String Method { get; set; }
        public String Protocol { get; set; }

        public RequestHeader() { }
        public RequestHeader(String hs):base(hs)
        {
            Regex r = new Regex(@"(.+) (.+) (.+)");
            Match m = r.Match(FirstLine);
            Method = m.Groups[1].Value;
            Path = m.Groups[2].Value;
            Protocol = m.Groups[3].Value;
        }

        override public String ToHttpHeader()
        {
            FirstLine = Method + " " + Path + " " + Protocol;
            return base.ToHttpHeader();
        }
    }
}
