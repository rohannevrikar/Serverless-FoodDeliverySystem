using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionApp.Orders.Startup))]

namespace FunctionApp.Orders
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Handle DIs if required
        }
    }
}