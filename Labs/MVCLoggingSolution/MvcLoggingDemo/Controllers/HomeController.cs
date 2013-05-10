using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MvcLoggingDemo.Services.Logging;

namespace MvcLoggingDemo.Controllers
{    
    public class HomeController : Controller
    {
        protected override void HandleUnknownAction(string actionName)
        {
            throw new HttpException(404, "Action not found");
        }

        public ActionResult Confused()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View("Error");
        }

        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            ILogger logger = LogFactory.Logger();
            logger.Info("INFO - We're on the Index page for Activities");
            logger.Debug("DEBUG - This is a debug message");
            logger.Fatal("FATAL - This is a fatal message");
            logger.Warn("WARN - This is a warning message");

            try
            {
                throw new Exception("A test exception");
            }
            catch (Exception ex)
            {
                logger.Error("ERROR - An error has occurred", ex);
            }     

            return View();
        }

        public ActionResult About()
        {
            // Throw a test error so that we can see that it is handled by Elmah
            // To test go to the ~/elmah.axd page to see if the error is being logged correctly
            throw new Exception("A test exception for ELMAH");

            return View();
        }
    }
}
