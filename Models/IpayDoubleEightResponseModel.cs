using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.IpayDoubleEight.Models
{
    /// <summary>
    /// Represent iPay88 response model
    /// </summary>
    public class IpayDoubleEightResponseModel
    {
        public string MerchantCode { get; set; }

        public int PaymentId { get; set; }

        public string RefNo { get; set; }

        public decimal Amount { get; set; }

        public string Remark { get; set; }

        public string TransId { get; set; }

        public string AuthCode { get; set; }

        public string Status { get; set; }

        public string ErrDesc { get; set; }

        public string Signature { get; set; }

        public string CCName { get; set; }

        public string CCNo { get; set; }

        public string S_bankname { get; set; }

        public string S_country { get; set; }
    }
}
