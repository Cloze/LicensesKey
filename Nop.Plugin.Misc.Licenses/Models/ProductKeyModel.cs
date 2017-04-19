using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.Licenses.Models
{
    public class ProductKeyModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Misc.Licenses.Fields.ProductKey")]
        public string ProductKey { get; set; }
    }
}