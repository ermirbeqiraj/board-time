using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MvcToDo.Models;

namespace MvcToDo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Just to be sure that the TaskMarks will always be there
            Initializer m = new Initializer();
            if (!m.MngeTaskMarksAndCategories())
            {
                /*
                 * redirect user to some page that shows him for the success failure :)
                 */
            }
            // Create roles if they are not present
            if (!m.MngeRoles())
            {
               /*
                 * redirect user to some page that shows him for failing successfully ;)
                 */
            }
        }
    }
}
