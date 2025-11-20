using Cysharp.Threading.Tasks;
using GameFx.Core;
using GameFx.Core.Bootstrap;
using GameFx.Core.Crypto;
using GameFx.Core.Serializer;
using GameFx.Features.SaveSystem;
using UnityEngine;

namespace GameFx
{
    [CreateAssetMenu(fileName = "SaveLoaderLoader", menuName = "GameFx/Loaders/SaveLoaderLoader")]
    public class SaveLoaderLoader : ScriptableLoader
    {
        protected override UniTask InternalLoad()
        {
            ISerializer serializer = new UnityJsonSerializer();
            ICrypto crypto = new Crypto("default_key_12345");

            ServiceLocator.Register<SaveLoader>(new SaveLoader(serializer, crypto));
            return UniTask.CompletedTask;
        }
    }
}