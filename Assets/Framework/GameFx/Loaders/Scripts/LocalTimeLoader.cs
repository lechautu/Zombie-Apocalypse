using Cysharp.Threading.Tasks;
using GameFx.Core;
using GameFx.Core.Bootstrap;
using GameFx.Core.Time;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace GameFx
{
    [CreateAssetMenu(fileName = "LocalTimeLoader", menuName = "GameFx/Loaders/LocalTimeLoader")]
    public class LocalTimeLoader : ScriptableLoader
    {
        protected override UniTask InternalLoad()
        {
            ServiceLocator.Register<ITimeService>(new LocalTimeService());
            return UniTask.CompletedTask;
        }
    }
}