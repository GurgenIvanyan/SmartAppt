using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
    public sealed class LoggerFactoryOptions
    {
        internal List<ILog> Sinks { get; } = new();
        internal Func<LogEntry, bool>? Filter { get; private set; }

        public LoggerFactoryOptions AddSink(ILog sink)
        {
            Sinks.Add(sink);
            return this;
        }

        public LoggerFactoryOptions SetFilter(Func<LogEntry, bool>? filter)
        {
            Filter = filter;
            return this;
        }
    }
}
