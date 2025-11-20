using System;
using System.Collections.Generic;
using GameFx.Core.PoolSystem;
using UnityEngine;

namespace GameFx.Core.PoolSystem
{
    [CreateAssetMenu(fileName = "ScriptablePool", menuName = "GameFx/Pools/ScriptablePool")]
    public class ScriptablePool : ScriptableObject
    {
        [SerializeField] GameObjectPool[] gameObjectPools;

        public void InitializeAllPools()
        {
            foreach (var pool in gameObjectPools)
            {
                pool.InitilizePool();
            }
        }
    }
}