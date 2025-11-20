using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFx.Core.Bootstrap
{
    public abstract class ScriptableLoader : ScriptableObject, ILoader
    {
        [SerializeField]
        protected List<ScriptableLoader> dependencies;
        public List<ILoader> Dependencies => dependencies.ConvertAll(d => (ILoader)d);

        [NonSerialized]
        private bool isLoaded = false;

        public bool IsLoaded => isLoaded;

        public async UniTask Load()
        {
            if (IsLoaded)
                return;

            List<UniTask> loadTasks = new();
            foreach (var dependency in Dependencies)
            {
                loadTasks.Add(UniTask.WaitUntil(() => dependency.IsLoaded));
            }
            await UniTask.WhenAll(loadTasks);

            await InternalLoad();
            isLoaded = true;
        }

        protected abstract UniTask InternalLoad();
    }
}