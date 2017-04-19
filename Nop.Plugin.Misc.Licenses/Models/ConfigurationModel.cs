using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Misc.Licenses.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public bool SavedSuccessfully { get; set; }
        public string SaveMessage { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.Licenses.Settings.UrlAttributeId")]
        public int UrlAttributeId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.Licenses.Settings.DomainAttributeId")]
        public int DomainAttributeId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.Licenses.Settings.LicenseType")]
        public int LicenseType { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.Licenses.Settings.Url")]
        public string Url { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.Licenses.Settings.ProductKey")]
        public string ProductKey { get; set; }

        public SelectList LicenseTypes { get; set; }
        public IList<SelectListItem> ProductAttributes { get; set; }

    }
}