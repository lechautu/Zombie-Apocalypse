using Cysharp.Threading.Tasks;
using GameFx.Core;
using GameFx.Core.Bootstrap;
using UnityEngine;

namespace GameFx
{
    [CreateAssetMenu(fileName = "EventDispatcherLoader", menuName = "GameFx/Loaders/EventDispatcherLoader")]
    public class EventDispatcherLoader: ScriptableLoader
    {
        protected override UniTask InternalLoad()
        {
            ServiceLocator.Register<EventDispatcher>(new EventDispatcher());
            return UniTask.CompletedTask;
        }
    }
}