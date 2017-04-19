using Nop.Plugin.Misc.Licenses.Controllers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using System;
using System.Text;
using System.Web.Mvc;

namespace Nop.Plugin.Misc.Licenses.Events
{
    /// <summary>
    /// This class is used to detect when a Product is being edited in order to add a new tab for our encryption key.
    /// These have to be unique for each Product so that someone doesn't purchase a license for one Product and then use it for others
    /// </summary>
    public class ProductKeyConsumer : IConsumer<AdminTabStripCreated>
    {
        private readonly IProductService _productService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public ProductKeyConsumer(IProductService productService, ILocalizationService localizationService, 
            IGenericAttributeService genericAttributeService, IPermissionService permissionService)
        {
            this._productService = productService;
            this._localizationService = localizationService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
        }

        public void HandleEvent(AdminTabStripCreated eventMessage)
        {
            if (eventMessage.TabStripName == "product-edit")
            {
                ProductKeyController controller = new ProductKeyController(_productService, _genericAttributeService, _permissionService);
                
                int productId = controller.GetProductId();
                string url = "/ProductKey/GetProductKey?productId=" + productId;
                string tabName = _localizationService.GetResource("Nop.Plugin.Misc.LicenseKey");
                //var sb = new StringBuilder();

                //sb.Append("<script language=\"javascript\" type=\"text/javascript\">");
                //sb.Append(Environment.NewLine);
                //sb.Append("$(document).ready(function () {");
                //sb.Append(Environment.NewLine);
                //sb.Append("$(\"<li><a data-tab-name='"  + tabName + "' data-toggle='tab' href='#" + tabName + "'>");
                //sb.Append(Environment.NewLine);
                //sb.Append(_localizationService.GetResource("Computer.Category.MenuIconTab"));
                //sb.Append(" kTabs.append({ text: \"" + tabName + "\", contentUrl: \"" + url + "\" });");
                //sb.Append(Environment.NewLine);
                //sb.Append("});");
                //sb.Append(Environment.NewLine);
                //sb.Append("</script>");
                //sb.Append(Environment.NewLine);
                //eventMessage.BlocksToRender.Add(MvcHtmlString.Create(sb.ToString()));
                eventMessage.BlocksToRender.Add(TabStripHelper.RenderAdminTab("product-edit",tabName,url));
            }
        }
    }
}
