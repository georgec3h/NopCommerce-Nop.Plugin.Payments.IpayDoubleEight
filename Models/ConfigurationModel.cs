using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.IpayDoubleEight.Models
{
    public class ConfigurationModel : BaseNopModel
    {

        #region Ctor

        public ConfigurationModel()
        {
            PaymentMethod = new List<int>();
            AvailablePayments = new List<SelectListItem>();
        }

        #endregion

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.MerchantCode")]
        public string MerchantCode { get; set; }
        public bool MerchantCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.MerchantKey")]
        public string MerchantKey { get; set; }
        public bool MerchantKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.IpayDoubleEightUrl")]
        public string IpayDoubleEightUrl { get; set; }
        public bool IpayDoubleEightUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.PaymentId")]
        public int PaymentId { get; set; }
        public bool PaymentId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.PaymentMethod")]
        public IList<int> PaymentMethod { get; set; }
        public IList<SelectListItem> AvailablePayments { get; set; }
        public bool PaymentMethod_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.Currency")]
        public string Currency { get; set; }
        public bool Currency_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.Lang")]
        public string Lang { get; set; }
        public bool Lang_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.SignatureType")]
        public string SignatureType { get; set; }
        public bool SignatureType_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.IpayDoubleEight.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
