using System;

namespace GameFx.Core.Log
{
    public struct LogEntry
    {
        public LogLevel Level;
        public string Message;
        public DateTime Timestamp;

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}";
        }
    }
}