using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.IpayDoubleEight.Helper;
using Nop.Plugin.Payments.IpayDoubleEight.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.IpayDoubleEight.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PaymentIpayDoubleEightController : BasePaymentController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentIpayDoubleEightController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utitlies

        protected string ParseMessageToNote(IpayDoubleEightResponseModel model)
        {
            if (model == null)
                return string.Empty;

            var note = new StringBuilder();
            note.AppendLine("iPay88 Result:");
            note.AppendLine($"MerchantCode:{model.MerchantCode}");
            note.AppendLine($"PaymentId:{model.PaymentId}");
            note.AppendLine($"Remark:{model.Remark}");
            note.AppendLine($"TransId:{model.TransId}");
            note.AppendLine($"AuthCode:{model.AuthCode}");
            note.AppendLine($"Status:{model.Status}");
            note.AppendLine($"ErrDesc:{model.ErrDesc}");
            note.AppendLine($"Signature:{model.Signature}");
            note.AppendLine($"CCName:{model.CCName}");
            note.AppendLine($"CCNo:{model.CCNo}");
            note.AppendLine($"S_bankname:{model.S_bankname}");
            note.AppendLine($"S_country:{model.S_country}");

            return note.ToString();
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var iapyDoubleEightPaymentSettings = _settingService.LoadSetting<IpayDoubleEightPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MerchantCode = iapyDoubleEightPaymentSettings.MerchantCode,
                MerchantKey = iapyDoubleEightPaymentSettings.MerchantKey,
                IpayDoubleEightUrl = iapyDoubleEightPaymentSettings.IpayDoubleEightUrl,
                PaymentId = iapyDoubleEightPaymentSettings.DefaultPaymentId,
                PaymentMethod = iapyDoubleEightPaymentSettings.PaymentMethod,
                Currency = iapyDoubleEightPaymentSettings.Currency,
                Lang = iapyDoubleEightPaymentSettings.Lang,
                SignatureType = iapyDoubleEightPaymentSettings.SignatureType,
                AdditionalFee = iapyDoubleEightPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = iapyDoubleEightPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.IpayDoubleEight/Views/Configure.cshtml", model);

            model.MerchantCode_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.MerchantCode, storeScope);
            model.MerchantKey_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.MerchantKey, storeScope);
            model.IpayDoubleEightUrl_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.IpayDoubleEightUrl, storeScope);
            model.PaymentId_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.DefaultPaymentId, storeScope);
            model.PaymentMethod_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.PaymentMethod, storeScope);
            model.Currency_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.Currency, storeScope);
            model.Lang_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.Lang, storeScope);
            model.SignatureType_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.SignatureType, storeScope);
            model.AdditionalFee_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(iapyDoubleEightPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            return View("~/Plugins/Payments.IpayDoubleEight/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var ipayDoubleEightPaymentSettings = _settingService.LoadSetting<IpayDoubleEightPaymentSettings>(storeScope);

            //save settings
            ipayDoubleEightPaymentSettings.MerchantCode = model.MerchantCode;
            ipayDoubleEightPaymentSettings.MerchantKey = model.MerchantKey;
            ipayDoubleEightPaymentSettings.IpayDoubleEightUrl = model.IpayDoubleEightUrl;
            ipayDoubleEightPaymentSettings.DefaultPaymentId = model.PaymentId;
            ipayDoubleEightPaymentSettings.PaymentMethod = model.PaymentMethod.ToList();
            ipayDoubleEightPaymentSettings.Currency = model.Currency;
            ipayDoubleEightPaymentSettings.Lang = model.Lang;
            ipayDoubleEightPaymentSettings.SignatureType = model.SignatureType;
            ipayDoubleEightPaymentSettings.AdditionalFee = model.AdditionalFee;
            ipayDoubleEightPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
            * This behavior can increase performance because cached settings will not be cleared 
            * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.MerchantCode, model.MerchantCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.MerchantKey, model.MerchantKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.IpayDoubleEightUrl, model.IpayDoubleEightUrl_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.DefaultPaymentId, model.PaymentId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.PaymentMethod, model.PaymentMethod_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.Currency, model.Currency_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.Lang, model.Lang_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.SignatureType, model.SignatureType_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ipayDoubleEightPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpsRequirement]
        public virtual IActionResult IpayBackend()
        {
            return Content("RECEIVEOK");
        }

        [HttpPost]
        public virtual IActionResult PaidCallback(IpayDoubleEightResponseModel model)
        {
            if (string.IsNullOrEmpty(model.RefNo))
                return RedirectToRoute("HomePage");

            var orderNumber = model.RefNo;
            if (model.RefNo.Contains("_"))
                orderNumber = orderNumber.Substring(0, model.RefNo.IndexOf('_'));

            var order = _orderService.GetOrderByCustomOrderNumber(orderNumber);
            if (order == null)
                return RedirectToRoute("HomePage");

            _orderService.InsertOrderNote(new OrderNote
            {
                OrderId = order.Id,
                Note = ParseMessageToNote(model),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            //check total 
            var orderTotalSentTo =
                _genericAttributeService.GetAttribute<decimal?>(order, IpayDoubleEightHelper.OrderTotalSentToIpayDoubleEight);
            if (orderTotalSentTo.HasValue && model.Amount != orderTotalSentTo.Value)
            {
                var errorStr =
                    $"iPay88. Returned order total {model.Amount} doesn't equal order total {order.OrderTotal}." +
                    $"Order #{order.CustomOrderNumber}.";
                //log
                _logger.Error(errorStr);
                //order note
                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                return RedirectToRoute("HomePage");
            }

            //clear attribute
            if (orderTotalSentTo.HasValue)
                _genericAttributeService.SaveAttribute<decimal?>(order, IpayDoubleEightHelper.OrderTotalSentToIpayDoubleEight, null);

            if (model.Status == "1" && _orderProcessingService.CanMarkOrderAsPaid(order))
            {
                order.AuthorizationTransactionId = model.TransId;
                order.AuthorizationTransactionCode = model.AuthCode;
                order.AuthorizationTransactionResult = model.Status;

                _orderService.UpdateOrder(order);

                _orderProcessingService.MarkOrderAsPaid(order);

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = model.ErrDesc,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }
        }
        #endregion
    }
}