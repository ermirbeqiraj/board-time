using System.Web;
using System.Web.Optimization;

namespace MvcToDo
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            /*
             * for jQueryUI
             */
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery-ui.unobtrusive-{version}.js"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                    "~/Content/themes/base/core.css",
                    "~/Content/themes/base/datepicker.css",
                    "~/Content/themes/base/sortable.css",
                    "~/Content/themes/base/theme.css"));

            /*
             * for SignalR and for the script that detects window focus
             */
            bundles.Add(new ScriptBundle("~/bundles/realtime").Include(
                "~/Scripts/jquery.signalR-{version}.js",
                "~/Scripts/tabVisibile.js",
                "~/Scripts/applicationGeneralScr.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/chatpage").Include(
                "~/Scripts/chatHelpers.js",
                "~/Scripts/left.menu.js"
                ));

            /*
             * for chat page
             */
            bundles.Add(new StyleBundle("~/Content/chat/css").Include(
                "~/Content/chat.css",
                "~/Content/leftMenu.css"));

            bundles.Add(new ScriptBundle("~/bundles/boardscript").Include(
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/boardScript.js",
                "~/Scripts/jquery-ui.unobtrusive-{version}.js"
                ));

        }
    }
}
