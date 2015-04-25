using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Sin.Http.Server
{
    public class StaticFileHandler:WebHandler
    {
        public String Prefix { get; set; }
        private Dictionary<String, String> _MIMEs = new Dictionary<string, string>() {
            {"css", "text/css"},
            {"htm", "text/html"},
            {"html", "text/html"},
            {"json", "text/json"},
            {"map", "text/plain"},
            {"js", "application/x-javascript"},
            {"bmp", "image/bmp"},
            {"gif", "image/gif"},
            {"ief", "image/ief"},
            {"jpe", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"jpg", "image/jpeg"},
            {"jfif", "image/pipeg"},
            {"svg", "image/svg+xml"},
            {"wav", "audio/x-wav"},
            {"*", "application/octet-stream"},
        };
        public Dictionary<String, String> MIMEs
        {
            get
            {
                return _MIMEs;
            }
        }
        public void GET(Context cxt)
        {
            Mutex m = new Mutex(false);

            GetFile(cxt, m);

            while (cxt.Response.Body == null)
            {
                Thread.Sleep(100);
            }
        }

        private async void GetFile(Context cxt, Mutex m)
        {
            try
            {
                String file = Prefix + cxt.PathArgs[1].Replace("/", "\\");
                String type = file.LastIndexOf('.') > 0 ? file.Substring(file.LastIndexOf('.') + 1) : "*";

                Debug.WriteLine("file: " + file);
                Windows.Storage.StorageFile sf = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(file);
                Stream st = await sf.OpenStreamForReadAsync();
                MemoryStream ms = new MemoryStream();
                st.CopyTo(ms);
                st.Close();
                byte[] dt = new byte[ms.Length];
                System.Array.Copy(ms.GetBuffer(), dt, dt.Length);
                ms.Close();

                if (MIMEs.ContainsKey(type))
                {
                    cxt.Response.Header["Content-Type"] = MIMEs[type];
                }
                else
                {
                    cxt.Response.Header["Content-Type"] = MIMEs["*"];
                }
                cxt.Response.Body = dt;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                cxt.Response.ResponseError(404, "Not Found");
            }
        }
        
    }
}
