using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFx.Core.Log
{
    public static class Logger
    {
        static readonly List<LogEntry> _logEntries = new();

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            var entry = new LogEntry
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.Now
            };

            _logEntries.Add(entry);

            switch (level)
            {
                case LogLevel.Error:
                    Debug.LogError(entry.ToString());
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(entry.ToString());
                    break;
                default:
                    Debug.Log(entry.ToString());
                    break;
            }
        }
    }
}