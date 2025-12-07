using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{

    public sealed class LogEntry
    {
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
        public LogLevel Level { get; init; }
        public string Category { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public Exception? Exception { get; init; }
        public object? Context { get; init; }
    }
}

