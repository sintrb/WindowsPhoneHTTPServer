# Windows Phone HTTP Server
A Simple HTTP Server Framework &amp; Demo for Windows Phone 8


#### How to use it
```C#
// Create Server at port 8080
Sin.Http.Server.Server Server = new Sin.Http.Server.Server() { Port = 8080 };


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
    </html>
    ";
    cxt.Response.Body = Encoding.UTF8.GetBytes(html);
});

// When /hi
Server.On("^/hi$", cxt =>
{
    cxt.Response.Body = Encoding.UTF8.GetBytes("Hello World.....");
});

//  Create a static file handler
// the files place at Assets\gh-pages
Sin.Http.Server.StaticFileHandler sfh = new Sin.Http.Server.StaticFileHandler()
{
    Prefix = "Assets\\gh-pages"
};

// handle all other request as file get request
Server.On("(.*)", sfh);

// at last
// Start it
Server.Start();
```

Then, use your PC broswer to open the url http://your-wp-ip:8080/ and Enjoy it.

Maybe you can add other function to it, please fork it first.

