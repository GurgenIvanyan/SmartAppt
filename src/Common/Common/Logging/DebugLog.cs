using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common.Logging
{
    public sealed class DebugLog : ILog
    {
        public void Write(LogEntry entry)
        {
            Debug.WriteLine(
                $"{entry.TimestampUtc:HH:mm:ss} [{entry.Level}] {entry.Category}: {entry.Message}");
        }
    }
}
