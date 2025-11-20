using Cysharp.Threading.Tasks;
using GameFx.Core.Bootstrap;
using UnityEngine;

namespace GameFx.Core.PoolSystem
{
    [CreateAssetMenu(fileName = "PoolingLoader", menuName = "GameFx/Loaders/PoolingLoader")]
    public class PoolingLoader : ScriptableLoader
    {
        [SerializeField]
        private ScriptablePool pool;

        protected override UniTask InternalLoad()
        {
            ServiceLocator.Register<PoolManager>(new PoolManager());
            pool.InitializeAllPools();
            return UniTask.CompletedTask;
        }
    }
}