using System.Web.Mvc;
using System.Web.Routing;

namespace ShareDataSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" });

            // any controller methods that are decorated with our attribute will be registered
            routes.MapMvcAttributeRoutes();

            // MUST be the last route as a catch-all!
            routes.MapRoute("404-PageNotFound", "{*url}", new { controller = "Home", action = "PageNotFound" });
        }
    }
}
