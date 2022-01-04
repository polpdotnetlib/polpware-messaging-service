using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    /// <summary>
    /// Indicates a fatal error. 
    /// When this happen, please check your logic or reclaim all resources
    /// to restart.
    /// </summary>
    public class UnexpectedConnectionDecoratorException : Exception
    {
    }
}
