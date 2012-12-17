using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace UploadAndDownload
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { action = "PostFile", id = RouteParameter.Optional }
            );
        }
    }
}
