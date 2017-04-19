using Nop.Plugin.Misc.Licenses.Domain;
using Nop.Plugin.Misc.Licenses.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using System;
using System.IO;
using System.Web.Mvc;

namespace Nop.Plugin.Misc.Licenses.Controllers
{
    [AdminAuthorize]
    public class ProductKeyController : Controller
    {
        private IProductService _productService;
        private IGenericAttributeService _genericAttributeService;
        private IPermissionService _permissionService;

        public ProductKeyController(IProductService productService, IGenericAttributeService genericAttributeService,
            IPermissionService permissionService)
        {
            this._productService = productService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
        }


        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public int GetProductId()
        {
            if (ControllerContext == null)
            {
                ControllerContext context = new ControllerContext(System.Web.HttpContext.Current.Request.RequestContext, this);
                ControllerContext = context;
            }
            int productId =  Convert.ToInt32(ControllerContext.RequestContext.RouteData.Values["id"]);
            return productId;
        }

        public ActionResult GetProductKey(int productId)
        {
            var model = new ProductKeyModel();
            var product = _productService.GetProductById(productId);
            model.ProductKey = product.GetAttribute<string>(Constants.LicenseKeyAttribute);

            return PartialView("~/Plugins/Misc.Licenses/Views/EditProductKey.cshtml", model);
        }


        public void SaveKey(int id, string productKey)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return;

            var product = _productService.GetProductById(id);
            _genericAttributeService.SaveAttribute<string>(product, Constants.LicenseKeyAttribute, productKey);
        }
    }
}