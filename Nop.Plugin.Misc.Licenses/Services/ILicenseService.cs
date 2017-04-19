using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Licenses.Domain;

namespace Nop.Plugin.Misc.Licenses.Services
{
    public interface ILicenseService
    {
        LicenseKey CreateLicense(OrderItem item);
        string CreateLicense(LicenseType type, string url, string key);
        int SendCustomerNotice(Order order, string licenseTable, int languageId);
    }
}
