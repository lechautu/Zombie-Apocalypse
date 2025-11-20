using System;
using Cysharp.Threading.Tasks;

namespace GameFx.Core.Time
{
    public class LocalTimeService : ITimeService
    {
        public DateTime CurrentTime => _timeAtStartup.AddSeconds(UnityEngine.Time.realtimeSinceStartup);
        
        DateTime _timeAtStartup;

        public LocalTimeService()
        {
            _timeAtStartup = DateTime.Now;
        }
    }
}