using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFx.Core.PoolSystem
{
    [Serializable]
    public class GameObjectPool : IPooling<GameObject>
    {
        [SerializeField] GameObject prefab;
        [SerializeField] int initialSize;

        List<GameObject> objectPool = new();
        Queue<GameObject> objectQueue = new();
        Transform parentTransform;

        public GameObjectPool(GameObject gameObject)
        {
            prefab = gameObject;
        }

        public GameObject GetObject()
        {
            if (objectQueue.Count > 0)
            {
                GameObject obj = objectQueue.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = UnityEngine.Object.Instantiate(prefab, parentTransform);
                objectPool.Add(newObj);
                return newObj;
            }
        }
        
        public void InitilizePool()
        {
            objectPool = new List<GameObject>();
            objectQueue = new Queue<GameObject>();
            ServiceLocator.Get<PoolManager>().RegisterPool(prefab, this);
            parentTransform = new GameObject(prefab.name + "_Pool").transform;
            UnityEngine.Object.DontDestroyOnLoad(parentTransform);
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = UnityEngine.Object.Instantiate(prefab, parentTransform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
                objectPool.Add(obj);
            }
        }

        public bool IsBelongedToPool(GameObject obj)
        {
            return objectPool.Contains(obj);
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(parentTransform);
            objectQueue.Enqueue(obj);
        }
    }
}