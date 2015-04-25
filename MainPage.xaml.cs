using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HTTPServer.Resources;

using System.ComponentModel;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Collections.ObjectModel;

using System.Text;

namespace HTTPServer
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        Sin.Http.Server.Server Server = new Sin.Http.Server.Server() { Port = 8080 }; // Create Server at port 8080

        ObservableCollection<String> _Logs = new ObservableCollection<string>();

        public ObservableCollection<String> Logs
        {
            get
            {
                return _Logs;
            }
        }

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            this.DataContext = this;

            
            // Config Router
            // When /
            Server.On("^/$", cxt =>
            {
                // Return a simple page
                String html = @"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='utf-8'>
                <title>Windows Phone HTTP Server</title>
                </head>
                <body>
                <h1>Welcome to Windows Phone HTTP Server</h1>
                <p>It's a simple web page, if you can saw this page, it's mean your Windows Phone was provided a HTTP service correct.<a href='/hi'>Say hi~~</a></p>
                    <a href='https://github.com/sintrb/WindowsPhoneHTTPServer'>https://github.com/sintrb/WindowsPhoneHTTPServer</a>
                </body>
                <p><a href='/index.html'>Open The Test Static Web Page</a></p>
                </html>
                ";
                cxt.Response.Body = Encoding.UTF8.GetBytes(html);

                LogReq(cxt);
            });

            // When /hi
            Server.On("^/hi$", cxt =>
            {
                cxt.Response.Body = Encoding.UTF8.GetBytes("Hello World.....");

                LogReq(cxt);
            });

            // When /err
            Server.On("^/err$", cxt =>
            {
                String s = "0x";
                // raise a exception
                int i = int.Parse(s);
                LogReq(cxt);
            });



            Dictionary<String, byte[]> Storages = new Dictionary<string, byte[]>();
            // When /storage/
            Server.On("^/storage/", cxt =>
            {
                if (cxt.Request.Header.Method == "GET")
                {
                    if (Storages.ContainsKey(cxt.Request.Header.Path))
                    {
                        cxt.Response.Body = Storages[cxt.Request.Header.Path];
                    }
                    else
                    {
                        cxt.Response.ResponseError(404, "Not Found");
                    }
                }
                else
                {
                    Storages[cxt.Request.Header.Path] = cxt.Request.Body;
                }

                LogReq(cxt);
            });

            //  StaticFileHandler: gh-pages
            Sin.Http.Server.StaticFileHandler sfh = new Sin.Http.Server.StaticFileHandler()
            {
                Prefix = "Assets\\gh-pages"
            };

            //Server.On("(.*)", sfh);

            // for log it, warp the sfh
            Server.On("(.*)", cxt =>
            {
                sfh.GET(cxt);
                LogReq(cxt);
            });

            // Start
            Server.Start();
        }

        private void AddLog(String log)
        {
            _Logs.Add(DateTime.Now.ToString("hh:mm:ss ") + log);
        }

        private void LogReq(Sin.Http.Server.Context cxt)
        {
            try
            {
                String s = cxt.Request.Header.Method + " " + cxt.Request.Header.Path + " " + cxt.Response.Header.Code + " " + cxt.Response.Header.Status;
                this.Dispatcher.BeginInvoke(() =>
                {
                    AddLog(s);
                });
            }
            catch { }
        }

        public bool KeepAlive
        {
            get
            {
                return Server.KeepAlive;
            }
            set
            {
                Server.KeepAlive = value;
            }
        }

        public String Port
        {
            get
            {
                return "" + Server.Port;
            }
            set
            {
                try
                {
                    Server.Port = Int32.Parse(value);
                }
                catch { }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(params String[] propertyNames)
        {
            if (null != PropertyChanged)
            {
                foreach (String propertyName in propertyNames)
                {
                    //Debug.WriteLine("NotifyPropertyChanged " + propertyName);
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Server.Status == Sin.Http.Server.ServerStatus.Running)
            {
                AddLog("Stop");
                Server.Stop();
                btnRunStop.Content = "Start";
            }
            else
            {
                AddLog("Start");
                Server.Start();
                btnRunStop.Content = "Stop";
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _Logs.Clear();
        }
    }
}