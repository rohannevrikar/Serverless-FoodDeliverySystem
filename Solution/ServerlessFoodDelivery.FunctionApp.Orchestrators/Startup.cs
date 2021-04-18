using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ServerlessFoodDelivery.FunctionApp.Orchestrators.Startup))]

namespace ServerlessFoodDelivery.FunctionApp.Orchestrators
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Handle DIs if required
        }
    }
}
