using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using UploadAndDownload.Models;

namespace UploadAndDownload.Controllers
{
    public class UploadController : ApiController
    {
        public async Task<HttpResponseMessage> PostFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = HttpContext.Current.Server.MapPath("~/app_data");
            var provider = new CustomMultipartFormDataStreamProvider(root);

            try
            {
                var sb = new StringBuilder();

                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var key in provider.FormData.AllKeys)
                {
                    foreach (var val in provider.FormData.GetValues(key))
                    {
                        sb.Append(string.Format("{0}:{1}\n", key, val));
                    }
                }

                foreach (var file in provider.FileData)
                {
                    var fileInfo = new FileInfo(file.LocalFileName);
                    sb.Append(string.Format("Uploaded file:{0}({1}bytes)\n", fileInfo.Name, fileInfo.Length));
                }

                return new HttpResponseMessage()
                    {
                        Content = new StringContent(sb.ToString())
                    };
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        public Task<IEnumerable<FileDesc>> Post()
        {
            string folderName = "app_data";
            string PATH = HttpContext.Current.Server.MapPath("~/" + folderName);
            string rootUrl = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.AbsolutePath, String.Empty);

            if (Request.Content.IsMimeMultipartContent())
            {
                var streamProvider = new CustomMultipartFormDataStreamProvider(PATH);
                var task = Request.Content.ReadAsMultipartAsync(streamProvider).ContinueWith<IEnumerable<FileDesc>>(t =>
                {

                    if (t.IsFaulted || t.IsCanceled)
                    {
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }

                    var fileInfo = streamProvider.FileData.Select(i =>
                    {
                        var info = new FileInfo(i.LocalFileName);
                        return new FileDesc(info.Name, rootUrl + "/" + folderName + "/" + info.Name, info.Length / 1024);
                    });
                    return fileInfo;
                });

                return task;
            }
            else
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));
            }

        }

    }

    public class FileDesc
    {
        public string name { get; set; }

        public string path { get; set; }

        public long size { get; set; }

        public FileDesc(string n, string p, long s)
        {
            name = n;
            path = p;
            size = s;
        }
    }

}
