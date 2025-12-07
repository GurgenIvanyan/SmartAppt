using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
   
    public static class AppLoggerFactory
        {
            private static readonly object _lock = new();
            private static readonly List<ILog> _sinks = new();
            private static Func<LogEntry, bool>? _filter;

            static AppLoggerFactory()
            {
                _sinks.Add(new DebugLog());
                _filter = null; 
            }

        
            public static void Configure(Action<LoggerFactoryOptions> configure)
            {
                lock (_lock)
                {
                    var opts = new LoggerFactoryOptions();
                    configure(opts);

                    _sinks.Clear();
                    _sinks.AddRange(opts.Sinks);

                    _filter = opts.Filter;
                }
            }

            internal static void WriteInternal(LogEntry entry)
            {
                lock (_lock)
                {
                    if (_filter != null && !_filter(entry))
                        return;

                    foreach (var sink in _sinks)
                    {
                        sink.Write(entry);
                    }
                }
            }

            public static ILogger CreateLogger<T>()
                => CreateLogger(typeof(T).FullName ?? typeof(T).Name);

            public static ILogger CreateLogger(string category)
                => new AppLogger(category);
        }
    }

