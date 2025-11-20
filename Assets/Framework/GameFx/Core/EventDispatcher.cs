using System;
using System.Collections.Generic;

namespace GameFx.Core
{
    public sealed class EventDispatcher
    {
        public class EventArgs
        {
            public Enum EventType;
            public object Data;
        }

        private readonly Dictionary<Enum, Action<EventArgs>> _eventHandlers = new();
        public void Dispatch(Enum eventType, object data = null)
        {
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType]?.Invoke(new EventArgs { EventType = eventType, Data = data });
            }
        }

        public void Subscribe(Enum eventType, Action<EventArgs> handler)
        {
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] += handler;
            }
            else
            {
                _eventHandlers[eventType] = handler;
            }
        }

        public void Unsubscribe(Enum eventType, Action<EventArgs> handler)
        {
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] -= handler;
            }
        }
    }
}