using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Licenses.Domain
{
    public class LicenseSettings : ISettings
    {
        public int UrlAttributeId { get; set; }
        public int DomainAttributeId { get; set; }
    }
}
