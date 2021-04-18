using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionApp.Restaurants.Startup))]

namespace FunctionApp.Restaurants
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Handle DIs if required
        }
    }
}