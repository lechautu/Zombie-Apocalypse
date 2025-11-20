using System;
using UnityEngine;

namespace GameFx.Core.Time
{

    public interface ITimeService
    {
        DateTime CurrentTime { get; }
    }
}

