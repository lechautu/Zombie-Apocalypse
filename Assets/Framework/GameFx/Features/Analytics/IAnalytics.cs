using System.Collections.Generic;

namespace GameFx.Features.Analytics
{
    public interface IAnalytics
    {
        void SetUserId(string userId);
        void SetUserProperty(string key, string value);
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
    }
}