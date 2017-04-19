using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Misc.Licenses
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Misc.Licenses.Configure",
                 "Plugins/Licenses/Configure",
                 new { controller = "License", action = "Configure" },
                 new[] { "Nop.Plugin.Misc.Licenses.Controllers" }
            );

            routes.MapRoute("Plugin.Misc.Licenses.SaveKey",
                 "Plugins/Licenses/SaveKey",
                 new { controller = "ProductKey", action = "SaveKey" },
                 new[] { "Nop.Plugin.Misc.Licenses.Controllers" }
            );

            routes.MapRoute("Plugin.Misc.Licenses.CreateLicenseKey",
                 "Plugins/Licenses/CreateKey",
                 new { controller = "License", action = "CreateLicense" },
                 new[] { "Nop.Plugin.Misc.Licenses.Controllers" }
            );
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
