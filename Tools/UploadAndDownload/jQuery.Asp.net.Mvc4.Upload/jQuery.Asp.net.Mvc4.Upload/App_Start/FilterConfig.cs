using System.Web;
using System.Web.Mvc;

namespace jQuery.Asp.net.Mvc4.Upload
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}