using System.Web.Routing;
using BootstrapMvcSample.Controllers;
using LogViewer.Controllers;
using NavigationRoutes;

namespace LogViewer.App_Start
{
    public class ExampleLayoutsRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapNavigationRoute<HomeController>("Labs", c => c.Index())
                .AddChildRoute<HomeController>("Labs-Chart", c => c.About(1));

            routes.MapNavigationRoute<ExampleLayoutsController>("Example Layouts", c => c.Starter())
                  .AddChildRoute<ExampleLayoutsController>("Marketing", c => c.Marketing())
                  .AddChildRoute<ExampleLayoutsController>("Fluid", c => c.Fluid())
                  .AddChildRoute<ExampleLayoutsController>("Sign In", c => c.SignIn())
                ;
        }
    }
}
