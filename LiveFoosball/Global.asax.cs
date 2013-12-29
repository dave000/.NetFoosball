using LiveFoosball.GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LiveFoosball
{
    public class MvcApplication : System.Web.HttpApplication
    {

        private static Logger log = LogManager.GetCurrentClassLogger();


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log.Info("Foosball app is starting");
            Task.Run(() => XivelyClient.Listener.Connect("ws://api.xively.com:8080", "1996686508/datastreams/Goal", Game.TrackGoal));
            log.Info("Foosball app is started");
        }
    }
}
