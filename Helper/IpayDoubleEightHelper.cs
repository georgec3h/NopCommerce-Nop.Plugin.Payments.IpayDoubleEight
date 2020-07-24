using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.IpayDoubleEight.Helper
{
    public static class IpayDoubleEightHelper
    {
        #region Utitlies

        /// <summary>
        /// Get string from hash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private static string GetStringFromHash(byte[] hash)
        {
            var result = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString().ToLower();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate sha 256 string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string GenerateSHA256String(string inputString)
        {
            var sha256 = SHA256Managed.Create();
            var bytes = Encoding.UTF8.GetBytes(inputString);
            var hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        /// <summary>
        /// Get the generic attribute name that is used to store an order total that actually sent to iPay88
        /// </summary>
        public static string OrderTotalSentToIpayDoubleEight => "OrderTotalSentToIpayDoubleEight";

        /// <summary>
        /// Get the generic attribute name that is used to store a payment that actually sent to iPay88
        /// </summary>
        public static string PaymentSentToIpayDoubleEight => "PaymentSentToIpayDoubleEight";

        #endregion
    }
}
