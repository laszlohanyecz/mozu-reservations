using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MozuProductReservationsReport
{
    public class MozuQuietLogger : global::Mozu.Api.Logging.ILogger, global::Mozu.Api.Logging.ILoggingService
    {
        public bool IsInfoEnabled { get { return false; } }
        public bool IsWarnEnabled { get { return false; } }
        public bool IsDebugEnabled { get { return false; } }
        public bool IsErrorEnabled { get { return false; } }
        public bool IsFatalEnabled { get { return false; } }

        public Task Info(object message, Exception ex = null, object properties = null)
        {
            return null;
        }

        public Task Warn(object message, Exception ex = null, object properties = null)
        {
            return null;
        }

        public Task Debug(object message, Exception ex = null, object properties = null)
        {
            return null;
        }

        public Task Error(object message, Exception ex = null, object properties = null)
        {
            return null;
        }

        public Task Fatal(object message, Exception ex = null, object properties = null)
        {
            return null;
        }

        public global::Mozu.Api.Logging.ILogger LoggerFor(Type type)
        {
            return new MozuQuietLogger();
        }
    }
}
