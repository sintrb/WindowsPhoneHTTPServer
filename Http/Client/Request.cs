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


        /// <summary>
        /// Request Method
        /// </summary>
        public String Method
        {
            get
            {
                return Header.Method;
            }
            set
            {
                Header.Method = value;
            }
        }

        /// <summary>
        /// Request Path
        /// </summary>
        public String Path
        {
            get
            {
                return Header.Path;
            }
            set
            {
                Header.Path = value;
            }
        }

        private Dictionary<String, String> _Parms =null;


        /// <summary>
        /// Parameters in URL
        /// </summary>
        public Dictionary<String, String> Parms
        {
            get
            {
                if (_Parms == null)
                {
                    _Parms = new Dictionary<string, string>();
                    if (Path != null && Path.IndexOf('?')>=0)
                    {
                        System.Text.RegularExpressions.Match m = new System.Text.RegularExpressions.Regex("([^=^&]+)=([^=^&]*)").Match(Path.Substring(Path.IndexOf('?') + 1));
                        while (m.Success)
                        {
                            _Parms[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim();
                            m = m.NextMatch();
                        }
                    }
                }
                return _Parms;
            }
        }

        private Dictionary<String, String> _Cookie = null;

        /// <summary>
        /// Cookie
        /// </summary>
        public Dictionary<String, String> Cookie
        {
            get
            {
                if (_Cookie == null)
                {
                    _Cookie = new Dictionary<string, string>();
                    if (Header.Headers.ContainsKey("Cookie"))
                    {
                        System.Text.RegularExpressions.Match m = new System.Text.RegularExpressions.Regex("([^=^;]+)=([^=^;]*)").Match(Header["Cookie"]);
                        while (m.Success)
                        {
                            _Cookie[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim();
                            m = m.NextMatch();
                        }
                    }
                }
                return _Cookie;
            }
        }
    }
}
