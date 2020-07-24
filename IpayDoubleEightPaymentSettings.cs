using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.IpayDoubleEight
{
    /// <summary>
    /// Represents settings of the iPay 88 payment plugin
    /// </summary>
    public class IpayDoubleEightPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the url
        /// </summary>
        public string IpayDoubleEightUrl { get; set; }

        /// <summary>
        /// Gets or sets the merchant code
        /// </summary>
        public string MerchantCode { get; set; }

        /// <summary>
        /// Gets or sets the merchant key
        /// </summary>
        public string MerchantKey { get; set; }
        
        /// <summary>
        /// Gets or sets the payment identifier
        /// </summary>
        public int DefaultPaymentId { get; set; }

        /// <summary>
        /// Gets or sets payment method
        /// </summary>
        public List<int> PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the lang
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// Gets or sets the signature type
        /// </summary>
        public string SignatureType { get; set; }

        /// <summary>
        /// Gets or sets additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
