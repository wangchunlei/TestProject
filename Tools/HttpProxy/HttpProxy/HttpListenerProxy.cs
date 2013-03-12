using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpProxy
{
    class RequestData
    {
        public HttpWebRequest WebRequest;
        public HttpListenerContext Context;

        public RequestData(HttpWebRequest request, HttpListenerContext context)
        {
            this.WebRequest = request;
            this.Context = context;
        }
    }

    public class HttpListenerProxy
    {
        static HttpListener listener = null;
        private HttpListenerProxy(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://*:{0}/", port));
        }
        public static HttpListenerProxy Create(int port)
        {
            if (!HttpListener.IsSupported)
            {
                throw new Exception("HttpListener is not supported!");
            }
            ServicePointManager.DefaultConnectionLimit = 1000;
            var proxy = new HttpListenerProxy(port);
            return proxy;
        }
        public void Start()
        {
            listener.Start();
            try
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("Listening...");
                        var context = listener.GetContext();//blocks while waiting for a request.
                        var requestString = context.Request.RawUrl;
                        var request = context.Request;
                        context.Request.Cookies.Add(new Cookie("localIp", context.Request.Headers["REMOTE_ADDR"]));
                        Console.WriteLine("Got request for " + requestString);
                        //var request = WebRequest.Create(requestString) as HttpWebRequest;
                        //request.KeepAlive = context.Request.KeepAlive;
                        //request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        //request.Method = context.Request.HttpMethod;
                        //request.ContentType = context.Request.ContentType;
                        //var cookies = new CookieContainer();
                        //cookies.Add(new Uri(requestString), context.Request.Cookies);
                        //request.CookieContainer = cookies;
                        //request.Timeout = 20000;

                        //var reqeustData = new RequestData(request, context);
                        //var webTask = Task.Factory.FromAsync<WebResponse>
                        //                (request.BeginGetResponse, request.EndGetResponse, null)
                        //              .ContinueWith(
                        //                task =>
                        //                {
                        //                    if (task.IsFaulted)
                        //                    {
                        //                        Console.WriteLine(task.Exception.GetBaseException());
                        //                    }
                        //                    else
                        //                    {
                        //                        var response = (HttpWebResponse)task.Result;
                        //                        RespCallback(reqeustData, response);
                        //                    }

                        //                });
                    }
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void Stop()
        {
            listener.Stop();
        }
        private void RespCallback(RequestData requestData, HttpWebResponse response)
        {
            try
            {
                Console.WriteLine("Got back response from " + requestData.Context.Request.Url.AbsoluteUri);

                using (response)
                using (Stream receiveStream = response.GetResponseStream())
                {
                    HttpListenerResponse responseOut = requestData.Context.Response;

                    // Need to get the length of the response before it can be forwarded on
                    responseOut.ContentLength64 = response.ContentLength;
                    int bytesCopied = CopyStream(receiveStream, responseOut.OutputStream);
                    responseOut.OutputStream.Close();

                    Console.WriteLine("Copied {0} bytes", bytesCopied);
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("\nMain Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nMain Exception raised!");
                Console.WriteLine("Source :{0} ", e.Source);
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        private int CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[1024 * 1024 * 10];
            int bytesWritten = 0;
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break;
                output.Write(buffer, 0, read);
                bytesWritten += read;
            }
            return bytesWritten;
        }
    }

}
