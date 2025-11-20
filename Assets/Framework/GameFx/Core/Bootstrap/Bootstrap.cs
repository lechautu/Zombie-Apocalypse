using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFx.Core.Bootstrap
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField]
        List<ScriptableLoader> rootLoaders;

        private readonly LoaderPipeline _loaderPipeline = new();

        void Start()
        {
            Load().Forget();
        }

        async UniTask Load()
        {
            await _loaderPipeline.BuildAsync(rootLoaders);
            await _loaderPipeline.ExecuteAsync();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}