using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Make sure the app is set up
            var k = KjeringiData.TheRace.Instance;

        }

        protected void Application_BeginRequest()
        {
            // get the original protocol and remove the header added by the AppHarbor loadbalancer
            var serverVariables = HttpContext.Current.Request.ServerVariables;
            var protocol = serverVariables["HTTP_X_FORWARDED_PROTO"];

            if (!String.IsNullOrEmpty(protocol))
            {
                serverVariables.Remove("HTTP_X_FORWARDED_PROTO");

                // fix the port and protocol
                var isHttps = "HTTPS".Equals(protocol, StringComparison.OrdinalIgnoreCase);
                serverVariables.Set("HTTPS", isHttps ? "on" : "off");
                serverVariables.Set("SERVER_PORT", isHttps ? "443" : "80");
                serverVariables.Set("SERVER_PORT_SECURE", isHttps ? "1" : "0");
            }
        }
    }
}
