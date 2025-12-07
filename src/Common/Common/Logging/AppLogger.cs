using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
    using System;

    
        internal sealed class AppLogger : ILogger   
        {
            private readonly string _category;

            internal AppLogger(string category)
            {
                _category = category ?? string.Empty;
            }

            public void Log(
                LogLevel level,
                string message,
                Exception? exception = null,
                object? context = null)
            {
                var entry = new LogEntry
                {
                    Level = level,
                    Category = _category,
                    Message = message,
                    Exception = exception,
                    Context = context
                };

                AppLoggerFactory.WriteInternal(entry);
            }

            public void Trace(string message, object? ctx = null)
                => Log(LogLevel.Trace, message, null, ctx);

            public void Debug(string message, object? ctx = null)
                => Log(LogLevel.Debug, message, null, ctx);

            public void Info(string message, object? ctx = null)
                => Log(LogLevel.Info, message, null, ctx);

            public void Warn(string message, object? ctx = null)
                => Log(LogLevel.Warning, message, null, ctx);

            public void Error(string message, Exception? ex = null, object? ctx = null)
                => Log(LogLevel.Error, message, ex, ctx);

            public void Critical(string message, Exception? ex = null, object? ctx = null)
                => Log(LogLevel.Critical, message, ex, ctx);

            internal static AppLogger For(string category)
                => new AppLogger(category);

            internal static AppLogger For<T>()
                => new AppLogger(typeof(T).FullName ?? typeof(T).Name);
        }
    }


