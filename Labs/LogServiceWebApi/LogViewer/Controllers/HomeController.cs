using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace LogViewer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            Log4netSqlite.Program.PersistentData();
            var data = Log4netSqlite.Program.Query("").ToList();
            return View(data);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Chart()
        {
            var basicChart = new Chart(width: 600, height: 400)
                .AddTitle("Chart Title")
                .AddSeries(
                    name: "Employee",
                    xValue: new[] { "Peter", "Andrew", "Julie", "Mary", "Dave" },
                    yValues: new[] { "2", "6", "4", "5", "3" }).GetBytes("png");
            return File(basicChart, "image/png");
        }
    }
}
