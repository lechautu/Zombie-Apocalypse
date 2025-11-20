using System;
using System.Collections.Generic;

namespace GameFx.Core
{

    public static class ServiceLocator
    {
        static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(object o) where T : class
        {
            if (_services.ContainsKey(typeof(T)))
            {
                throw new Exception("Service of type " + typeof(T).ToString() + " is already registered.");
            }

            _services[typeof(T)] = o;
        }

        public static void Unregister<T>() where T : class
        {
            if (!_services.ContainsKey(typeof(T)))
            {
                throw new Exception("Service of type " + typeof(T).ToString() + " is not registered.");
            }

            _services.Remove(typeof(T));
        }

        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public static T Get<T>() where T : class
        {
            if (!_services.ContainsKey(typeof(T)))
            {
                throw new Exception("Service of type " + typeof(T).ToString() + " is not registered.");
            }

            return (T)_services[typeof(T)];
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            service = null;

            if (!_services.ContainsKey(typeof(T)))
            {
                return false;
            }

            service = (T)_services[typeof(T)];
            return true;
        }
    }
}