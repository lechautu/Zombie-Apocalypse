using System.Collections.Generic;
using UnityEngine;

namespace GameFx.Core.PoolSystem
{
    public class PoolManager
    {
        private Dictionary<object, object> pools;

        public PoolManager()
        {
            pools = new();
        }

        public void RegisterPool<T>(IPooling<T> pool) where T : class
        {
            var type = typeof(T);
            if (!pools.ContainsKey(type))
            {
                pools[type] = pool;
            }
        }

        public IPooling<T> GetPool<T>() where T : class
        {
            var type = typeof(T);
            if (pools.TryGetValue(type, out var pool))
            {
                return pool as IPooling<T>;
            }
            return null;
        }

        public void RegisterPool<T>(T key, IPooling<T> pool) where T : class
        {
            if (!pools.ContainsKey(key))
            {
                pools[key] = pool;
            }
        }

        public IPooling<T> GetPool<T>(T key) where T : class
        {
            if (pools.TryGetValue(key, out var pool))
            {
                return pool as IPooling<T>;
            }
            else if (typeof(T) == typeof(GameObject))
            {
                var newPool = new GameObjectPool((GameObject)(object)key);
                newPool.InitilizePool();
                return newPool as IPooling<T>;
            }
            return null;
        }

        public void ReturnToPool<T>(T _object) where T : class
        {
            foreach (var item in pools.Values)
            {
                if (item is IPooling<T> pool && pool.IsBelongedToPool(_object))
                {
                    pool.ReturnObject(_object);
                }
            }
        }
    }
}