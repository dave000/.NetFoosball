﻿using LiveFoosball.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LiveFoosball
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "GoalApi", 
            //    routeTemplate: "api/foosball/goal/{team}", 
            //    defaults: new { controller = "foosball", action = "Goal" });
        

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{action}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            }
    }
}
