using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Shared
{
    public static class ExceptionHandler
    {
        public static void LogExceptionDetails(this Exception exception, ILogger log, string orderId, string className)
        {            
            log.LogError($"Something went wrong in {className}.\r\nOrder ID: {orderId}.\r\nException details: {exception.ToString()}");
        }
    }
}
