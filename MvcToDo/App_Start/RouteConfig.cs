using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcToDo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            /*
             * a route to map tasks , because we must see tasks that are specific
             * to a project, we need a url like that below
             */
            routes.MapRoute(
                name: "TaskItemFiles",
                url: "Project-{projectId}/{controller}/{action}/{id}",
                defaults: new { controller = "TaskItems", action = "Board", id = UrlParameter.Optional}
                );
            /*
             * the general route.
             */
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Projects", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
