using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.IpayDoubleEight.Helper;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.IpayDoubleEight
{
    /// <summary>
    /// iPay 88 payment processor
    /// </summary>
    public class IpayDoubleEightPaymentProcessor : BasePlugin,IPaymentMethod
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IpayDoubleEightPaymentSettings _ipayDoubleEightPaymentSettings;

        #endregion

        #region Ctor

        public IpayDoubleEightPaymentProcessor(
            IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService, 
            IOrderService orderService, 
            IPaymentService paymentService,
            ISettingService settingService, 
            IWebHelper webHelper, 
            IpayDoubleEightPaymentSettings ipayDoubleEightPaymentSettings)
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _orderService = orderService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _ipayDoubleEightPaymentSettings = ipayDoubleEightPaymentSettings;
        }

        #endregion

        #region Utitlies

        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, string> CreateQueryParameters()
        {
            return new Dictionary<string, string>
            {
                ["MerchantCode"] = _ipayDoubleEightPaymentSettings.MerchantCode,
                ["PaymentId"] = _ipayDoubleEightPaymentSettings.ToString(),
                ["Currency"] = _ipayDoubleEightPaymentSettings.Currency,
                ["Remark"] = "-",
                ["Lang"] = _ipayDoubleEightPaymentSettings.Lang,
                ["SignatureType"] = _ipayDoubleEightPaymentSettings.SignatureType,
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {   
            //create common query parameters for the request
            var queryParameters = CreateQueryParameters();

            var order = postProcessPaymentRequest.Order;
            var refNo = $"{order.CustomOrderNumber}_{_orderService.GetOrderNotesByOrderId(order.Id).Count}";
            queryParameters.Add("RefNo", refNo);

            var orderTotal = order.OrderTotal;
            var roundedOrderTotal = Math.Round(orderTotal, 2);
            var amount = roundedOrderTotal.ToString("#,#.00", CultureInfo.InvariantCulture);
            var amountForHash = new string(amount.Where(char.IsDigit).ToArray());
            queryParameters.Add("Amount", amount);
            _genericAttributeService.SaveAttribute(order, IpayDoubleEightHelper.OrderTotalSentToIpayDoubleEight, roundedOrderTotal);
            queryParameters.Add("Signature", IpayDoubleEightHelper.GenerateSHA256String($"{_ipayDoubleEightPaymentSettings.MerchantKey}" +
                                                                                        $"{_ipayDoubleEightPaymentSettings.MerchantCode}" +
                                                                                        $"{refNo}" +
                                                                                        $"{amountForHash}" +
                                                                                        $"{_ipayDoubleEightPaymentSettings.Currency}"));

            var orderItem = _orderService.GetOrderItems(order.Id).First();
            var product = _orderService.GetProductByOrderItemId(orderItem.Id);
            queryParameters.Add("ProdDesc", product.Name);

            //choosing correct order address
            var orderAddress = _addressService.GetAddressById(
                (postProcessPaymentRequest.Order.PickupInStore ? postProcessPaymentRequest.Order.PickupAddressId : postProcessPaymentRequest.Order.ShippingAddressId) ?? 0);

            queryParameters.Add("UserName", $"{orderAddress.FirstName}{orderAddress.LastName}");
            queryParameters.Add("UserEmail", orderAddress.Email);
            queryParameters.Add("UserContact", orderAddress.PhoneNumber);

            queryParameters.Add("ResponseURL", $"{_webHelper.GetStoreLocation()}{IpayDoubleEightPaymentDefault.IpayDoubleEightCallbackRouteUrl}");
            queryParameters.Add("BackendURL", $"{_webHelper.GetStoreLocation()}{IpayDoubleEightPaymentDefault.IpayDoubleEightBackendRouteUrl}");

            //prepare post
            var form = new RemotePost { FormName = "ePayment", Url = _ipayDoubleEightPaymentSettings.IpayDoubleEightUrl };

            foreach (var queryParameter in queryParameters)
            {
                form.Add(queryParameter.Key, queryParameter.Value);
            }

            //post
            form.Post();
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _ipayDoubleEightPaymentSettings.AdditionalFee, _ipayDoubleEightPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }
            
        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentIpayDoubleEight/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentIpayDoubleEight";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new IpayDoubleEightPaymentSettings
            {
                IpayDoubleEightUrl = "https://payment.ipay88.com.my/epayment/entry.asp"
            });

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payments.IpayDoubleEight.PaymentMethod.Description"] = "iPay88",
                ["Plugins.Payments.IpayDoubleEight.Fields.RedirectionTip"] = "Please select payment first,then you will be redirected to iPay88 site to complete the order.",
                ["Plugins.Payments.IpayDoubleEight.Fields.MerchantCode"] = "MerchantCode",
                ["Plugins.Payments.IpayDoubleEight.Fields.MerchantKey"] = "MerchantKey",
                ["Plugins.Payments.IpayDoubleEight.Fields.IpayDoubleEightUrl"] = "Url",
                ["Plugins.Payments.IpayDoubleEight.Fields.PaymentId"] = "PaymentId",
                ["Plugins.Payments.IpayDoubleEight.Fields.PaymentId.Hint"] = "If PaymentId not post via request, gateway will choose the predefined default payment method.By default,payment method can be re - select by customer from iPay88 payment.",
                ["Plugins.Payments.IpayDoubleEight.Fields.PaymentMethod"] = "Enable payment methods",
                ["Plugins.Payments.IpayDoubleEight.Fields.Currency"] = "Currency",
                ["Plugins.Payments.IpayDoubleEight.Fields.Lang"] = "Lang",
                ["Plugins.Payments.IpayDoubleEight.Fields.SignatureType"] = "Signature Type",
                ["Plugins.Payments.IpayDoubleEight.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.IpayDoubleEight.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.IpayDoubleEight.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.IpayDoubleEight.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.IpayDoubleEight.PaymentMethodDescription"] = "You will be redirected to iPay88 site to complete the payment",
                ["Plugins.Payments.IpayDoubleEight.Instructions"] = @"
                    <p>
	                    <b>If you're using this gateway ensure that your primary store currency is supported by iPay88.</b>
	                    <br />
	                    <br />1. Log in to your iPay88 account (click <a href=""https://www.mobile88.com/epayment/report/"" target=""_blank"">here</a> to create your account).
	                    <br />
                    </p>",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<IpayDoubleEightPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Payments.IpayDoubleEight");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.IpayDoubleEight.PaymentMethodDescription");


        #endregion

    }
}
