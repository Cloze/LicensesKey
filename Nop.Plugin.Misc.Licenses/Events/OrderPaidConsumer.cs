using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Licenses.Domain;
using Nop.Plugin.Misc.Licenses.Services;
using Nop.Services.Events;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.Licenses.Events
{
    /// <summary>
    /// This class is used to detect when a Product is being edited in order to add a new tab for our encryption key.
    /// These have to be unique for each Product so that someone doesn't purchase a license for one Product and then use it for others
    /// </summary>
    public class OrderPaidConsumer : IConsumer<OrderPaidEvent>
    {
        private readonly ILicenseService _licenseService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        public OrderPaidConsumer(ILicenseService licenseService, IOrderService orderService,
            IWorkContext workContext)
        {
            this._licenseService = licenseService;
            this._orderService = orderService;
            this._workContext = workContext;
        }

        public void HandleEvent(OrderPaidEvent eventMessage)
        {
            IList<LicenseKey> licenses = new List<LicenseKey>();
            foreach (var item in eventMessage.Order.OrderItems)
            {
                var license = _licenseService.CreateLicense(item);
                if (!String.IsNullOrEmpty(license.License))
                {
                    licenses.Add(license);
                }
            }

            if (licenses.Count > 0)
            {
                //create info table
                StringBuilder sb = new StringBuilder();
                sb.Append("<table cellspacing=\"5\"><tr><th>Type</th><th>Product</th><th>Url</th><th>License</th></tr>");
                foreach (var license in licenses)
                {
                    sb.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", license.LicenseType, license.ProductName, license.Url, license.License));
                }
                sb.Append("</table>");
                string licenseTable = sb.ToString();

                sb = new StringBuilder();
                sb.Append("License Keys: " + Environment.NewLine);
                foreach (var license in licenses)
                {
                    sb.Append(string.Format("{0} - {1}: {2}{3}", license.ProductName, license.Url, license.License, Environment.NewLine));
                }
                string licensePlainText = sb.ToString();

                //add to order notes
                eventMessage.Order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = true,
                    Note = licensePlainText,
                    OrderId = eventMessage.Order.Id,
                });
                _orderService.UpdateOrder(eventMessage.Order);

                //send email
                var emailId = _licenseService.SendCustomerNotice(eventMessage.Order, licenseTable, _workContext.WorkingLanguage.Id);
                eventMessage.Order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = "\"License Key\" email (to customer) has been queued. Queued email identifier: "+emailId.ToString()+".",
                    OrderId = eventMessage.Order.Id
                });
                _orderService.UpdateOrder(eventMessage.Order);
            }
        }
    }
}
