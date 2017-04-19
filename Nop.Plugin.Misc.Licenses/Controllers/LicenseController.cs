using Nop.Plugin.Misc.Licenses.Domain;
using Nop.Plugin.Misc.Licenses.Models;
using Nop.Plugin.Misc.Licenses.Services;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Nop.Plugin.Misc.Licenses.Controllers
{
    [AdminAuthorize]
    public class LicenseController : BasePluginController
    {
        private IPermissionService _permissionService;
        private IProductAttributeService _productAttributeService;
        private ISettingService _settingService;
        private LicenseSettings _licenseSettings;
        private ILicenseService _licenseService;
        private ILocalizationService _localizationService;

        public LicenseController(IProductService productService, IGenericAttributeService genericAttributeService,
            IPermissionService permissionService, LicenseSettings licenseSettings,
            IProductAttributeService productAttributeService, ISettingService settingService,
            ILicenseService licenseService, ILocalizationService localizationService)
        {
            this._permissionService = permissionService;
            this._licenseSettings = licenseSettings;
            this._productAttributeService = productAttributeService;
            this._settingService = settingService;
            this._licenseService = licenseService;
            this._localizationService = localizationService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();

            model.UrlAttributeId = _licenseSettings.UrlAttributeId;
            model.DomainAttributeId = _licenseSettings.DomainAttributeId;
            model.ProductAttributes = _productAttributeService.GetAllProductAttributes().Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
            model.ProductAttributes.Insert(0, new SelectListItem { Value = "0", Text = "None" });
            model.LicenseTypes = LicenseType.Url.ToSelectList();
            
            return View("~/Plugins/Misc.Licenses/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("url-attribute")]
        public ActionResult UrlAttribute(ConfigurationModel model)
        {
            model.SavedSuccessfully = true;
            if (ModelState.IsValid)
            {
                try
                {
                    _licenseSettings.UrlAttributeId = model.UrlAttributeId;
                    _licenseSettings.DomainAttributeId = model.DomainAttributeId;

                    _settingService.SaveSetting(_licenseSettings);

                    SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
                    return Configure();
                }
                catch (Exception ex)
                {
                    ErrorNotification(ex.Message);
                    return Configure();
                }
            }
            else
            {
                return Configure();
            }
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("create-license")]
        public ActionResult CreateLicense(ConfigurationModel model)
        {
            if (String.IsNullOrEmpty(model.ProductKey) || model.ProductKey.Length < 16)
            {
                ErrorNotification( _localizationService.GetResource("Nop.Plugin.Misc.Licenses.Settings.ProductKey.Hint"));
                return Configure();
            }

            try
            {
                var createKey =  _licenseService.CreateLicense((LicenseType)model.LicenseType, model.Url, model.ProductKey);
                SuccessNotification(createKey);
                return Configure();
            }
            catch (Exception ex)
            {
                ErrorNotification(ex.Message);
                return Configure();
            }
        }
    }
}