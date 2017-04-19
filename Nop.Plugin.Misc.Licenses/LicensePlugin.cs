using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using System.Web.Routing;

namespace Nop.Plugin.Misc.Licenses
{
    public class LicensePlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private IMessageTemplateService _messageTemplateService;
        private IStoreContext _storeContext;

        #endregion

        #region Ctor

        public LicensePlugin(IMessageTemplateService messageTemplateService, IStoreContext storeContext)
        {
            this._messageTemplateService = messageTemplateService;
            this._storeContext = storeContext;
        }

        #endregion

        #region Methods

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.LicenseKey", "License Key");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Fields.ProductKey", "License Encryption Key");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Fields.ProductKey.Hint", "Used with the customer's entry to create the unique license key, MUST BE AT LEAST 16 characters");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.UrlAttributeId", "Url Attribute");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.DomainAttributeId", "Domain Attribute");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.UrlAttributeId.Hint", "If an ordered product has this attribute, a License will be created and sent");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.DomainAttributeId.Hint", "If an ordered product has this attribute, a License will be created and sent");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.LicenseKey.Saved", "Saved");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.LicenseKey.ManuallyCreate", "Manually create a license key");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.LicenseType", "License Type");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.LicenseType.Hint", "Url only works for that one and www. while domain works for any subdomains of that url");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.Url", "Url");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.Url.Hint", "The host name without http://.  Leave off www as well so it can work with both.");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.ProductKey", "Product Key");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.ProductKey.Hint", "Product Key must be at least 16 characters.  Must also match the one in the plugin that the license is for.");

            _messageTemplateService.InsertMessageTemplate(new MessageTemplate
            {
                Name = "Nop.Misc.Licenses.Customer.SendLicense",
                Subject = "	%Store.Name%: License key(s)",
                Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Here are the license keys you recently ordered <br /><br />Order Number: %Order.OrderNumber%<br />Date Ordered: %Order.CreatedOn%<br />Licenses:<br />%LicenseTable%</p>",
                IsActive = true,
                LimitedToStores = false
            });

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.LicenseKey");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Fields.ProductKey");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Fields.ProductKey.Hint");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.UrlAttributeId");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.DomainAttributeId");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.UrlAttributeId.Hint");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.DomainAttributeId.Hint");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.LicenseKey.Saved");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.LicenseKey.ManuallyCreate");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.LicenseType");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.LicenseType.Hint");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.Url");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.Url.Hint");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.ProductKey");
            this.DeletePluginLocaleResource("Nop.Plugin.Misc.Licenses.Settings.ProductKey.Hint");

            var template = _messageTemplateService.GetMessageTemplateByName("Nop.Misc.Licenses.Customer.SendLicense", _storeContext.CurrentStore.Id);
            if (template != null)
            {
                _messageTemplateService.DeleteMessageTemplate(template);
            }

            base.Uninstall();
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "License";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Misc.Licenses.Controllers" }, { "area", null } };
        }

        #endregion
    }
}
