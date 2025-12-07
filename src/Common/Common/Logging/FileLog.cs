using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Common.Logging
{
       public sealed class FileLog : ILog
        {
            private readonly string _directory;
            private readonly string _fileNamePattern;
            private readonly object _lock = new();

            public FileLog(
                string directory,
                string fileNamePattern = "smartappt-{0:yyyy-MM-dd}.log")
            {
                _directory = directory;
                _fileNamePattern = fileNamePattern;
                Directory.CreateDirectory(_directory);
            }

            public void Write(LogEntry entry)
            {
                lock (_lock)
                {
                    var fileName = string.Format(_fileNamePattern, entry.TimestampUtc);
                    var fullPath = Path.Combine(_directory, fileName);

                    var line =
                        $"{entry.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff} UTC " +
                        $"[{entry.Level}] [{entry.Category}] {entry.Message}";

                    if (entry.Context != null)
                    {
                        var json = JsonSerializer.Serialize(entry.Context);
                        line += " | ctx=" + json;
                    }

                    if (entry.Exception != null)
                    {
                        line += Environment.NewLine + entry.Exception;
                    }

                    File.AppendAllText(fullPath, line + Environment.NewLine);
                }
            }
        }
    }


