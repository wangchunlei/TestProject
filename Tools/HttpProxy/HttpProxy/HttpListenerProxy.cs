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
                while (true)
                {
                    Console.WriteLine("Listening...");
                    var context = listener.GetContext();//blocks while waiting for a request.
                    var requestString = context.Request.RawUrl;
                    //var request = context.Request;
                    Console.WriteLine("Got request for " + requestString);

                    try
                    {

                        var request = WebRequest.Create(requestString) as HttpWebRequest;
                        request.AllowAutoRedirect = false;
                        request.KeepAlive = context.Request.KeepAlive;
                        request.Proxy = new WebProxy();
                        request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        request.Method = context.Request.HttpMethod;
                        request.ContentType = context.Request.ContentType;

                        var cookies = new CookieContainer();
                        cookies.Add(new Uri(requestString), context.Request.Cookies);
                        request.CookieContainer = cookies;
                        request.Timeout = 20000;

                        var rs = new RequestData(request, context);
                        if (request.Method != "GET")
                        {
                            request.ContentLength = context.Request.ContentLength64;
                            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), rs);
                        }
                        else
                        {
                            IAsyncResult result = (IAsyncResult)request.BeginGetResponse(
                            new AsyncCallback(RespCallback), rs);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void GetRequestStreamCallback(IAsyncResult ar)
        {
            var rs = (RequestData)ar.AsyncState;
            HttpWebRequest request = rs.WebRequest;

            // End the operation
            Stream postStream = request.EndGetRequestStream(ar);

            rs.Context.Request.InputStream.CopyTo(postStream);
            postStream.Close();
            IAsyncResult result = (IAsyncResult)request.BeginGetResponse(
                            new AsyncCallback(RespCallback), rs);
        }

        private void RespCallback(IAsyncResult ar)
        {
            var rs = (RequestData)ar.AsyncState;
            try
            {
                WebRequest req = rs.WebRequest;
                HttpWebResponse resp = req.EndGetResponse(ar) as HttpWebResponse;

                //rs.Context.Response.Headers = resp.Headers;
                if (resp.ContentLength > 0)
                {
                    rs.Context.Response.ContentLength64 = resp.ContentLength;
                }

                rs.Context.Response.ContentType = resp.ContentType;
                //rs.Context.Response.Headers[HttpResponseHeader.SetCookie] = resp.Headers[HttpResponseHeader.SetCookie];

                foreach (Cookie cookie in resp.Cookies)
                {
                    if (cookie.Expired)
                    {
                        rs.Context.Response.Headers.Add(HttpResponseHeader.SetCookie,
                        string.Format("{0}={1}; expires={2}; path={3}", cookie.Name, cookie.Value, cookie.Expires.ToString("R"), "/"));

                    }
                    else
                    {
                        rs.Context.Response.Headers.Add(HttpResponseHeader.SetCookie,
                        string.Format("{0}={1}; path={2}", cookie.Name, cookie.Value, "/"));

                    }
                }
                var statusCode = resp.StatusCode;
                if (statusCode == HttpStatusCode.Redirect)
                {
                    rs.Context.Response.StatusCode = (int)statusCode;
                    rs.Context.Response.RedirectLocation = resp.Headers["Location"];
                }

                resp.GetResponseStream().CopyTo(rs.Context.Response.OutputStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                rs.Context.Response.OutputStream.Close();
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
