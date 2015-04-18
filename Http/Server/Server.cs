using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Sin.Http.Server
{
    public enum ClientStatus
    {
        WaitHeader,
        WaitBody,
        Completed   // Request Completed
    }

    public class ClientObject
    {
        public Client.RequestHeader Header = null;
        public List<byte> Buffer = new List<byte>();
        public ClientStatus Status = ClientStatus.WaitHeader;
        public SocketAsyncEventArgs EventArgs { get; set; }
    }

    public enum ServerStatus
    {
        Running,
        Stoped
    }
    public class Context
    {
        public Client.Request Request;
        public Response Response;
    }

    public delegate void RequestHandler(Context cxt);
    public class Server
    {
        public String Address { get; set; }
        public int Port {get;set;}
        public int BufferSize { get; set; }
        public int BackLog { get; set; }
        public bool KeepAlive { get; set; }

        public ServerStatus _Status;

        /// <summary>
        /// Server Status
        /// </summary>
        public ServerStatus Status
        {
            get
            {
                if (_Status == ServerStatus.Running && (ServerSocket == null || ServerSocket.LocalEndPoint == null))
                    _Status = ServerStatus.Stoped;
                return _Status;
            }
        }

        public Server()
        {
            Address = "0.0.0.0";
            Port = 8080;
            BufferSize = 1024;
            BackLog = 16;
            KeepAlive = true;
        }


        /// <summary>
        /// Start Server
        /// </summary>
        public void Start()
        {
            if (ServerSocket == null || ServerSocket.LocalEndPoint == null)
            {
                _Status = ServerStatus.Running;
                SocketAsyncEventArgs_AcceptCompleted(null, null);
            }
        }


        /// <summary>
        /// Stop Server
        /// </summary>
        public void Stop()
        {
            if (ServerSocket != null && ServerSocket.LocalEndPoint != null)
            {
                _Status = ServerStatus.Stoped;
                ServerSocket.Close();
                ServerSocket = null;
            }
        }

        private System.Net.Sockets.Socket ServerSocket = null;
        private Dictionary<String, ClientObject> Clients = new Dictionary<string, ClientObject>();
        void SocketAsyncEventArgs_SendCompleted(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            String key = e.UserToken as String;
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                // Send Completed
                if(!KeepAlive)
                    e.AcceptSocket.Close();
            }
        }

        void SocketAsyncEventArgs_ReceiveCompleted(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            String key = e.UserToken as String;

            if (e.Buffer != null && e.AcceptSocket != null && e.AcceptSocket.Connected && e.AcceptSocket.RemoteEndPoint != null)
            {
                try
                {
                    if (e.BytesTransferred > 0)
                    {
                        Socket soc = e.AcceptSocket;
                        ClientObject co = null;
                        if (Clients.ContainsKey(key))
                        {
                            co = Clients[key];
                        }
                        else
                        {
                            co = new ClientObject()
                            {
                                EventArgs = e
                            };
                            Clients[key] = co;
                        }
                        if (e.LastOperation == SocketAsyncOperation.Receive)
                        {
                            //Debug.WriteLine(key + " R:" + e.BytesTransferred);

                            switch (co.Status)
                            {
                                case ClientStatus.WaitHeader:
                                    {
                                        int start = Math.Max(0, co.Buffer.Count - Base.DoubleNewLineBytes.Length - 1);
                                        StringBuilder sb = new StringBuilder();
                                        for (int i = 0; i < e.BytesTransferred; ++i)
                                        {
                                            //sb.Append(String.Format("{0} ", e.Buffer[e.Offset + i]));
                                            co.Buffer.Add(e.Buffer[e.Offset + i]);
                                        }
                                        //Debug.WriteLine(sb);
                                        int ix = co.Buffer.IndexOfBytes(Base.DoubleNewLineBytes, start);
                                        if (ix > 0)
                                        {
                                            int nstart = ix + Base.DoubleNewLineBytes.Length;
                                            byte[] bytes = co.Buffer.ToArray();
                                            co.Buffer.Clear();

                                            for (int i = nstart; i < bytes.Length; ++i)
                                            {
                                                co.Buffer.Add(bytes[i]);
                                            }

                                            Client.RequestHeader Header = new Client.RequestHeader(Encoding.UTF8.GetString(bytes, 0, nstart));
                                            co.Header = Header;
                                            co.Status = co.Buffer.Count < co.Header.ContentLength ? ClientStatus.WaitBody : ClientStatus.Completed;
                                        }
                                        break;
                                    }
                                case ClientStatus.WaitBody:
                                    {
                                        for (int i = 0; i < e.BytesTransferred; ++i)
                                        {
                                            co.Buffer.Add(e.Buffer[e.Offset + i]);
                                        }
                                        if (co.Buffer.Count >= co.Header.ContentLength)
                                            co.Status = ClientStatus.Completed;
                                        break;
                                    }
                            }

                            if (co.Status == ClientStatus.Completed)
                            {
                                // 
                                Client.Request Req = new Client.Request()
                                {
                                    Header = co.Header,
                                };
                                ProcessRequest(co);
                                co.Header = null;
                                co.Status = ClientStatus.WaitHeader;
                                co.Buffer.Clear();
                            }
                            soc.ReceiveAsync(co.EventArgs);
                        }
                    }
                    else
                    {
                        // closed
                        Debug.WriteLine(key + ":" + "Close");
                        Clients.Remove(key);
                    }
                }
                catch
                {
                    Clients.Remove(key);
                }
            }
        }

        void SocketAsyncEventArgs_AcceptCompleted(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            if (e != null)
            {
                if (e.AcceptSocket != null && e.AcceptSocket.RemoteEndPoint != null && e.LastOperation == SocketAsyncOperation.Accept)
                {
                    Debug.WriteLine(e.AcceptSocket.RemoteEndPoint.ToString());
                    System.Net.Sockets.SocketAsyncEventArgs saea = new System.Net.Sockets.SocketAsyncEventArgs();
                    saea.Completed += SocketAsyncEventArgs_ReceiveCompleted;
                    saea.AcceptSocket = e.AcceptSocket;
                    saea.UserToken = e.AcceptSocket.RemoteEndPoint.ToString();
                    saea.SetBuffer(new byte[BufferSize], 0, BufferSize);
                    e.AcceptSocket.ReceiveAsync(saea);
                }
                else
                {
                    Debug.WriteLine("Stop");
                }
            }


            bool Crashed = false;
            try
            {
                Crashed = ServerSocket == null || ServerSocket.LocalEndPoint == null;
            }
            catch { }

            if (Crashed && _Status == ServerStatus.Running)
            {
                Clients.Clear();

                ServerSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(Address), Port));
                ServerSocket.Listen(BackLog);
                Debug.WriteLine(String.Format("Listen at {0}:{1}", Address, Port));
                // 
                SocketAsyncEventArgs_AcceptCompleted(null, null);
            }
            else if (ServerSocket != null)
            {
                System.Net.Sockets.SocketAsyncEventArgs saea = new System.Net.Sockets.SocketAsyncEventArgs();
                saea.Completed += SocketAsyncEventArgs_AcceptCompleted;
                ServerSocket.AcceptAsync(saea);
            }
        }

        private void ProcessRequest(ClientObject co)
        {
            Client.Request Req = new Client.Request()
            {
                Header = co.Header,
                Body = co.Buffer.Count>0?co.Buffer.ToArray():null,
                Client = co.EventArgs.AcceptSocket
            };
            Response Res = HandleRequest(Req);

            Res.Header.ContentLength = Res.Body != null ? Res.Body.Length : 0;
            Res.Header["Connection"] = KeepAlive ? "KeepAlive" : "Close";


            List<byte> buf = new List<byte>();
            buf.AddRange(Encoding.UTF8.GetBytes(Res.Header.ToHttpHeader()));
            buf.AddRange(Base.NewLineBytes);
            if (Res.Body != null && Res.Body.Length > 0)
            {
                buf.AddRange(Res.Body);
            }
            byte[] bts = buf.ToArray();
            System.Net.Sockets.SocketAsyncEventArgs saea = new System.Net.Sockets.SocketAsyncEventArgs();
            saea.Completed += SocketAsyncEventArgs_SendCompleted;
            saea.SetBuffer(bts, 0, bts.Length);
            saea.AcceptSocket = co.EventArgs.AcceptSocket;
            saea.UserToken = co.EventArgs.UserToken;
            co.EventArgs.AcceptSocket.SendAsync(saea);
            Debug.WriteLine(Req.Header.Method + " " + Req.Header.Path + " " + Res.Header.Code + " " + Res.Header.Status);
        }


        private Dictionary<String, RequestHandler> Handlers = new Dictionary<string, RequestHandler>();
        public void On(String Path, RequestHandler handler)
        {
            Handlers[Path] = handler;
        }

        virtual public RequestHandler GetHandler(String Path)
        {
            if (Handlers.ContainsKey(Path))
                return Handlers[Path];
            foreach (var kv in Handlers)
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(kv.Key);
                if (r.IsMatch(Path))
                {
                    return kv.Value;
                }
            }
            return null;
        }

        virtual public Response HandleRequest(Client.Request Req)
        {
            Response Res = new Response()
            {
                Header = new ResponseHeader()
                {
                    Code = 200,
                    Status = "OK",
                    Protocol = Req.Header.Protocol
                }
            };
            Res.Header["Server"] = "WPServer";
            Res.Header["Content-Type"] = "text/html";

            Context cxt = new Context()
            {
                Request = Req,
                Response = Res
            };


            RequestHandler rh = GetHandler(Req.Header.Path);
            if (rh != null)
            {
                try
                {
                    rh(cxt);
                }
                catch (Exception e)
                {
                    Res.Header.Code = 500;
                    Res.Header.Status = "Server Error";
                    Res.Body = Encoding.UTF8.GetBytes(String.Format("<html><body><pre>{0}</pre></body></html>", e.Message + "\r\n" + e.StackTrace));
                }
            }
            else
            {
                Res.Header.Code = 404;
                Res.Header.Status = "Not Found";
                Res.Body = Encoding.UTF8.GetBytes(String.Format("<html><body><center>{0}</center></body></html>", "404 Not Found"));
            }
            return cxt.Response;
        }
    }
}
