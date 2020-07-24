using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.IpayDoubleEight.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //Call back
            endpointRouteBuilder.MapControllerRoute(IpayDoubleEightPaymentDefault.IpayDoubleEightCallbackRouteName,
                IpayDoubleEightPaymentDefault.IpayDoubleEightCallbackRouteUrl,
                 new { controller = "PaymentIpayDoubleEight", action = "PaidCallback" });

            //backend
            endpointRouteBuilder.MapControllerRoute(IpayDoubleEightPaymentDefault.IpayDoubleEightBackendRoute,
                IpayDoubleEightPaymentDefault.IpayDoubleEightBackendRouteUrl,
                new { controller = "PaymentIpayDoubleEight", action = "IpayBackend" });

        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}