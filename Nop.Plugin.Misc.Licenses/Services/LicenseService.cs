using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Licenses.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Misc.Licenses.Services
{
    public class LicenseService : ILicenseService
    {
        private IGenericAttributeService _genericAttributeService;
        private IProductAttributeParser _productAttributeParser;
        private IStoreService _storeService;
        private IStoreContext _storeContext;
        private ILanguageService _languageService;
        private IMessageTokenProvider _messageTokenProvider;
        private ITokenizer _tokenizer;
        private IQueuedEmailService _queuedEmailService;
        private IEmailAccountService _emailAccountService;
        private IMessageTemplateService _messageTemplateService;

        private LicenseSettings _licenseSettings;
        private EmailAccountSettings _emailAccountSettings;

        public LicenseService(IGenericAttributeService genericAttributeService, IProductAttributeParser productAttributeParser,
            LicenseSettings licenseSettings, IStoreService storeService,
            ILanguageService languageService, IStoreContext storeContext,
            IMessageTokenProvider messageTokenProvider, ITokenizer tokenizer,
            IQueuedEmailService queuedEmailService, IEmailAccountService emailAccountService,
            IMessageTemplateService messageTemplateService, EmailAccountSettings emailAccountSettings)
        {
            this._genericAttributeService = genericAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._storeService = storeService;
            this._languageService = languageService;
            this._storeContext = storeContext;
            this._messageTokenProvider = messageTokenProvider;
            this._tokenizer = tokenizer;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountService = emailAccountService;
            this._messageTemplateService = messageTemplateService;

            this._licenseSettings = licenseSettings;
            this._emailAccountSettings = emailAccountSettings;
        }

        /// <summary>
        /// If the Order Item has one of the marked Attributes, create a License string and return it
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public LicenseKey CreateLicense(OrderItem item)
        {
            LicenseKey license = new LicenseKey();
            license.ProductName = item.Product.GetLocalized(x => x.Name);

            var productKey = item.Product.GetAttribute<string>(Constants.LicenseKeyAttribute);
            if(productKey == null)
            {
                return license;
            }

            if (_licenseSettings.DomainAttributeId != 0)
            {
                //let's see if this product has our Domain attribute
                var pva = _productAttributeParser.ParseProductAttributeMappings(item.AttributesXml).Where(a => a.ProductAttributeId == _licenseSettings.DomainAttributeId).FirstOrDefault();
                if (pva != null)
                {
                    var url = _productAttributeParser.ParseValues(item.AttributesXml, pva.Id).First();
                    license.Url = url;
                    license.LicenseType = LicenseType.Domain.ToString();
                    license.License = CreateLicense(LicenseType.Domain, url, productKey);
                    return license;
                }
            }

            if (_licenseSettings.UrlAttributeId != 0)
            {
                var pva = _productAttributeParser.ParseProductAttributeMappings(item.AttributesXml).Where(a => a.ProductAttributeId == _licenseSettings.UrlAttributeId).FirstOrDefault();
                if (pva != null)
                {
                    var url = _productAttributeParser.ParseValues(item.AttributesXml, pva.Id).First();
                    license.Url = url;
                    license.LicenseType = LicenseType.Url.ToString();
                    license.License = CreateLicense(LicenseType.Url, url, productKey);
                    return license;
                }
            }

            return license;
        }

        public string CreateLicense(LicenseType type, string url, string key)
        {
            url = (type == LicenseType.Url ? "U" : "D") + url;
            return EncryptText(url, key);
        }

        public int SendCustomerNotice(Order order, string licenseTable, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetLocalizedActiveMessageTemplate("Nop.Misc.Licenses.Customer.SendLicense", languageId, store.Id);
            if (messageTemplate == null)
                return 0;
            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);
            tokens.Add(new Token("LicenseTable", licenseTable, true));

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }


        #region Private 

        private string EncryptText(string plainText, string encryptionPrivateKey)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8));

            byte[] encryptedBinary = EncryptTextToMemory(plainText, tDESalg.Key, tDESalg.IV);
            return Convert.ToBase64String(encryptedBinary);
        }

        private byte[] EncryptTextToMemory(string data, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    byte[] toEncrypt = new UnicodeEncoding().GetBytes(data);
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }

        private int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName)
        {
            //retrieve localized message template data
            var bcc = messageTemplate.GetLocalized((mt) => mt.BccEmailAddresses, languageId);
            var subject = messageTemplate.GetLocalized((mt) => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized((mt) => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var email = new QueuedEmail()
            {
                //Priority = 5,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }

        private int EnsureLanguageIsActive(int languageId, int storeId)
        {
            //load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = _languageService.GetAllLanguages(storeId: storeId).FirstOrDefault();
            }
            if (language == null || !language.Published)
            {
                //load any language
                language = _languageService.GetAllLanguages().FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");
            return language.Id;
        }

        private MessageTemplate GetLocalizedActiveMessageTemplate(string messageTemplateName,
            int languageId, int storeId)
        {
            //TODO remove languageId parameter
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        private EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccounId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccounId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;

        }

        #endregion
    }
}
