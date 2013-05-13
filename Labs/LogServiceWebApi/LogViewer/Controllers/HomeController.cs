using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace LogViewer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About(int page = 1)
        {
            const int pageSize = 2;
            Log4netSqlite.Program.PersistentData();
            var data = Log4netSqlite.Program.Query("").ToList();
            var viewData = new
                {
                    PageSize = pageSize,
                    PageNumber = page,
                    Products = data.Skip((page - 1) * pageSize).Take(pageSize),
                    TotalRows = data.Count
                };
            return View(ToExpando(viewData));
        }
        public static ExpandoObject ToExpando(object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Chart()
        {
            var basicChart = new Chart(width: 440, height: 400)
                .AddTitle("Chart Title")
                .AddSeries(
                    name: "Employee",
                    xValue: new[] { "Peter", "Andrew", "Julie", "Mary", "Dave" },
                    yValues: new[] { "2", "6", "4", "5", "3" }).GetBytes("png");
            return File(basicChart, "image/png");
        }

        public ActionResult MyChart()
        {
            var dataSet = new DataSet();
            dataSet.ReadXmlSchema(Server.MapPath("~/App_Data/data.xsd"));
            dataSet.ReadXml(Server.MapPath("~/App_Data/data.xml"));
            var dataView = new DataView(dataSet.Tables[0]);

            var myChart = new Chart(width: 440, height: 400)
                .AddTitle("Sales Per Employee")
                .AddSeries("Default", chartType: "Pie",
                    xValue: dataView, xField: "Name",
                    yValues: dataView, yFields: "Sales").GetBytes("png");
            return File(myChart, "image/png");
        }
    }
}
