using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ServerlessFoodDelivery.Shared.Services;

[assembly: FunctionsStartup(typeof(FunctionApp.Orders.Startup))]

namespace FunctionApp.Orders
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICosmosHelper, CosmosHelper>();
            builder.Services.AddSingleton<ISettingService, SettingService>();
            builder.Services.AddSingleton<ICustomerService, CustomerService>();
            builder.Services.AddSingleton<IStorageService, StorageService>();
            builder.Services.AddSingleton<IOrderService, OrderService>();
            builder.Services.AddSingleton<IRestaurantService, RestaurantService>();
        }
    }
}