using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ServerlessFoodDelivery.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(ServerlessFoodDelivery.FunctionApp.Orchestrators.Startup))]

namespace ServerlessFoodDelivery.FunctionApp.Orchestrators
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {     
            builder.Services.AddSingleton<IOrderService, OrderService>();
        }
    }
}
