using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
    
        public interface ILogger
        {
            void Log(
                LogLevel level,
                string message,
                Exception? exception = null,
                object? context = null);

            void Trace(string message, object? ctx = null);
            void Debug(string message, object? ctx = null);
            void Info(string message, object? ctx = null);
            void Warn(string message, object? ctx = null);
            void Error(string message, Exception? ex = null, object? ctx = null);
            void Critical(string message, Exception? ex = null, object? ctx = null);
        }
    }


