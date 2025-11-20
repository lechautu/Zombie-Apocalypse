using UnityEngine;

namespace GameFx.Core.PoolSystem
{
    public interface IPooling<T> where T : class
    {
        void InitilizePool();
        T GetObject();
        void ReturnObject(T obj);
        bool IsBelongedToPool(T obj);
    }
}