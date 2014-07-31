using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpRequestTest
{
    class Program
    {
        static void Main(string[] args)
        {
        }
        //public static object CallWebService(string webServiceAsmxUrl, string serviceName, string methodName, object[] args)
        //{
        //    var client = new System.Net.WebClient();
        //    var stream = client.OpenRead(webServiceAsmxUrl + "?wsdl");
        //    var description = ServiceDescription.Read(stream);

        //    var importer = new ServiceDescriptionImporter();
        //    importer.ProtocolName = "Soap12";
        //    importer.AddServiceDescription(description, null, null);

        //    importer.Style = ServiceDescriptionImportStyle.Client;

        //    importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

        //    var nmspace = new CodeNamespace();
        //    var unit = new CodeCompileUnit();
        //    unit.Namespaces.Add(nmspace);

        //    var warming = importer.Import(nmspace, unit);

        //    if (warming == 0)
        //    {
        //        var provider1 = CodeDomProvider.CreateProvider("C#");
        //        var assemblyReferences = new string[]{"System.dll", 
        //        "System.Web.Services.dll", "System.Web.dll", 
        //        "System.Xml.dll", "System.Data.dll"};

        //        var parms = new CompilerParameters(assemblyReferences);
        //        parms.GenerateInMemory = true;
        //        var results = provider1.CompileAssemblyFromDom(parms, unit);

        //        if (results.Errors.Count > 0)
        //        {
        //            foreach (CompilerError oops in results.Errors)
        //            {
        //                System.Diagnostics.Debug.WriteLine("========Compiler error============");
        //                System.Diagnostics.Debug.WriteLine(oops.ErrorText);
        //            }

        //            throw new System.Exception("Compile Error Occurred calling webservice.");
        //        }

        //        var wsvcClass = results.CompiledAssembly.CreateInstance(serviceName);
        //        var mi = wsvcClass.GetType().GetMethod(methodName);
        //        return mi.Invoke(wsvcClass, args);
        //    }
        //    return null;
        //}
        //private static async Task HttpsInvokeByHttpClient()
        //{
        //    var httpclient = new HttpClient();
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        //    HttpResponseMessage response = await httpclient.GetAsync("https://218.189.25.166/");
        //    var content = response.Content;
        //}

        private static void SocktHttp()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("localhost", 80);
            string getRequest = "POST /api/AddMonitorLogAPI/AddMonitorLog HTTP/1.1\r\nHost: server225.anquan.com:8090\r\nConnection: keep-alive\r\nContent-Length: 571\r\n\r\nAccept: text/html\r\nUser-Agent: CSharpTests\r\n\r\njsonStr=  {\"BusinessType\": 1,  \"PrinterName\":\"HPLaserJet 1020\",  \"DriverName\": \"HP LaserJet 1020\",  \"PortName\": \"USB001\",  \"ProcessorName\": \"ZIMFPrint\",  \"FileName\": \"C:\\Users\\menglingfen\\Desktop\\test.txt\",  \"JobID\": \"30\",  \"TotalPages\":\"1\",  \"TotalBytes\": \"16024\",  \"SubmitOn\": \"2014-05-16 13:53: 8\",  \"SubmitBy\": \"menglingfen\",  \"Copies\": \"1\",  \"Co lor\": \"??????\",  \"Duplex\": \"?????????\",  \"MachineName\": \"MENGLINGFEN\",  \"MacAddress\": \"E8-9A-8F-8A-20-E8\",  \"IPAdd ress\": \"192.168.1.216\",  \"PaperType\": \"A4\",  \"ZoomInfo\": \"1\",  \"Extension0\": null,  \"Extension1\": null  }\r\n";

            socket.Send(Encoding.ASCII.GetBytes(getRequest));

            bool flag = true; // just so we know we are still reading
            string headerString = ""; // to store header information
            int contentLength = 0; // the body length
            byte[] bodyBuff = new byte[0]; // to later hold the body content
            while (flag)
            {
                // read the header byte by byte, until \r\n\r\n
                byte[] buffer = new byte[1];
                socket.Receive(buffer, 0, 1, 0);
                headerString += Encoding.ASCII.GetString(buffer);
                if (headerString.Contains("\r\n\r\n"))
                {
                    // header is received, parsing content length
                    // I use regular expressions, but any other method you can think of is ok
                    Regex reg = new Regex("\\\r\nContent-Length: (.*?)\\\r\n");
                    Match m = reg.Match(headerString);
                    contentLength = int.Parse(m.Groups[1].ToString());
                    flag = false;
                    // read the body
                    bodyBuff = new byte[contentLength];
                    socket.Receive(bodyBuff, 0, contentLength, 0);
                }
            }
            Console.WriteLine("Server Response :");
            string body = Encoding.ASCII.GetString(bodyBuff);
            Console.WriteLine(body);
            socket.Close();
        }
    }
}
