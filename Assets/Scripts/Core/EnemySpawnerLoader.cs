using UnityEngine;
using GameFx.Core.Bootstrap;
using Cysharp.Threading.Tasks;
using GameFx.Core;
using Enemy;

[CreateAssetMenu(fileName = "EnemySpawnerLoader", menuName = "Game/Loaders/EnemySpawner")]
public class EnemySpawnerLoader : ScriptableLoader
{
    [SerializeField] GameObject prefab;

    protected override UniTask InternalLoad()
    {
        var go = Instantiate(prefab);
        DontDestroyOnLoad(go);
        ServiceLocator.Register<ZombieSpawner>(go.GetComponent<ZombieSpawner>());
        return UniTask.CompletedTask;
    }
}
