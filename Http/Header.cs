using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http
{
    public class Header
    {
        public String FirstLine = null;
        public Dictionary<String, String> Headers = new Dictionary<string,string>();

        public String this[String k]
        {
            get
            {
                return Headers[k];
            }
            set
            {
                Headers[k] = value;
            }
        }

        private const String KeyContentLength = "Content-Length";
        public long ContentLength
        {
            get
            {
                if (Headers.ContainsKey(KeyContentLength))
                    return long.Parse(Headers[KeyContentLength]);
                else
                    return 0;
            }
            set
            {
                Headers[KeyContentLength] = "" + value;
            }
        }
        public void Clear()
        {
            Headers.Clear();
            FirstLine = null;
        }

        public Header()
        { }
        public Header(String hs)
        {
            foreach (String s in hs.Split(new String[] { Base.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (FirstLine == null)
                {
                    FirstLine = s;
                    continue;
                }

                int ix = s.IndexOf(':');
                Headers[s.Substring(0, ix).Trim()] = s.Substring(ix + 1).Trim();
            }
        }

        virtual public String ToHttpHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FirstLine);
            sb.Append(Base.NewLine);
            foreach (String k in Headers.Keys)
            {
                sb.Append(k);
                sb.Append(":");
                sb.Append(Headers[k]);
                sb.Append(Base.NewLine);
            }

            return sb.ToString();
        }
    }
}

