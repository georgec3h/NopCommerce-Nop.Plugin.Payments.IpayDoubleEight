using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.IpayDoubleEight
{
    public class IpayDoubleEightPaymentDefault
    {
        /// <summary>
        /// Name of the view component to display plugin in public store
        /// </summary>
        public const string VIEW_COMPONENT_NAME = "PaymentIpayDoubleEight";

        /// <summary>
        /// Name of the iPay88 call back route name
        /// </summary>
        public static string IpayDoubleEightCallbackRouteName => "Plugin.Payments.IpayDoubleEight.Result";

        /// <summary>
        /// Name of the iPay88 call back route url
        /// </summary>
        public static string IpayDoubleEightCallbackRouteUrl => "Plugins/PaymentIpayDoubleEight/Callback";

        /// <summary>
        /// Name of the route to the order ipay eight eight success callback
        /// </summary>
        public static string IpayDoubleEightBackendRoute => "Plugin.Payments.IpayDoubletEight.Backend";

        /// <summary>
        /// Name of the route to the order ipay eight eight success callback url
        /// </summary>
        public static string IpayDoubleEightBackendRouteUrl => "Plugins/PaymentIpayDoubleEight/Backend";

    }
}
