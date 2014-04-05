using ImpersonForIIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ImpersonForIIS
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
           
        }
        protected void Application_AuthenticateRequest(Object sender, EventArgs e) { }
        protected void Application_AuthorizeRequest(Object sender, EventArgs e) { }
        protected void Application_ResolveRequestCache(Object sender, EventArgs e) { }
        protected void Application_AcquireRequestState(Object sender, EventArgs e) { }
        protected void Application_PreRequestHandlerExecute(Object sender, EventArgs e) { }
        protected void Application_PreSendRequestHeaders(Object sender, EventArgs e) { }
        protected void Application_PreSendRequestContent(Object sender, EventArgs e) { }
        //<<code is executed>>(Object sender, EventArgs e){}
        protected void Application_PostRequestHandlerExecute(Object sender, EventArgs e) { }
        protected void Application_ReleaseRequestState(Object sender, EventArgs e) { }
        protected void Application_UpdateRequestCache(Object sender, EventArgs e) { }
        protected void Application_EndRequest(Object sender, EventArgs e) { }
    }
}
