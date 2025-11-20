using UnityEngine;
using Enemy;
using GameFx.Core;

public class DifficultyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScriptableLevelConfig levelConfigAsset;
    [SerializeField] private int startLevelIndex = 0;

    private LevelConfig _currentLevel;
    private int _currentLevelIndex;
    private int _currentWaveIndex;
    private int _aliveEnemiesInWave;
    private ZombieSpawner _enemySpawner;

    private EventDispatcher _eventDispatcher;

    private void Awake()
    {
        _enemySpawner = ServiceLocator.Get<ZombieSpawner>();
        _eventDispatcher = ServiceLocator.Get<EventDispatcher>();
    }

    private void OnEnable()
    {
        if (_eventDispatcher == null)
            _eventDispatcher = ServiceLocator.Get<EventDispatcher>();

        if (_eventDispatcher != null)
        {
            _eventDispatcher.Subscribe(EventConstants.OnEnemyKilled, OnEnemyKilled);
        }
        else
        {
            Debug.LogError("[DifficultyController] EventDispatcher not found in ServiceLocator.");
        }
    }

    private void OnDisable()
    {
        if (_eventDispatcher != null)
        {
            _eventDispatcher.Unsubscribe(EventConstants.OnEnemyKilled, OnEnemyKilled);
        }
    }

    private void Start()
    {
        StartLevel(startLevelIndex);
    }

    public void StartLevel(int levelIndex)
    {
        if (_enemySpawner == null || levelConfigAsset == null)
        {
            Debug.LogError("[DifficultyController] Missing references.");
            return;
        }

        var level = levelConfigAsset.GetClampedLevelConfig(levelIndex);
        if (level == null || level.waves == null || level.waves.Count == 0)
        {
            Debug.LogError($"[DifficultyController] Invalid LevelConfig for index {levelIndex}");
            return;
        }

        _currentLevel = level;
        _currentLevelIndex = levelIndex;
        _currentWaveIndex = 0;

        StartCurrentWave();
    }

    private void StartCurrentWave()
    {
        if (_currentLevel == null)
        {
            Debug.LogError("[DifficultyController] No current level.");
            return;
        }

        if (_currentWaveIndex < 0 || _currentWaveIndex >= _currentLevel.waves.Count)
        {
            Debug.Log($"[DifficultyController] Level {_currentLevelIndex} completed.");
            OnLevelCompleted();
            return;
        }

        WaveConfig wave = _currentLevel.waves[_currentWaveIndex];

        _aliveEnemiesInWave = CountEnemiesInWave(wave);
        if (_aliveEnemiesInWave <= 0)
        {
            Debug.LogWarning("[DifficultyController] Wave has 0 enemies, skipping.");
            GoToNextWave();
            return;
        }

        Debug.Log($"[DifficultyController] Level {_currentLevelIndex}, Wave {_currentWaveIndex} started. Enemies: {_aliveEnemiesInWave}");

        // Only here we tell the spawner to actually spawn
        _enemySpawner.SpawnWave(wave);
    }

    private int CountEnemiesInWave(WaveConfig wave)
    {
        int total = 0;
        if (wave?.entries != null)
        {
            foreach (var entry in wave.entries)
            {
                total += Mathf.Max(0, entry.amount);
            }
        }
        return total;
    }

    // EventDispatcher callback
    private void OnEnemyKilled(EventDispatcher.EventArgs args)
    {
        // args.EventType == EventConstants.OnEnemyKilled
        // args.Data can contain the enemy object if you dispatched it

        if (_aliveEnemiesInWave <= 0)
            return;

        _aliveEnemiesInWave--;

        if (_aliveEnemiesInWave <= 0)
        {
            Debug.Log($"[DifficultyController] Wave {_currentWaveIndex} cleared.");
            GoToNextWave();
        }
    }

    private void GoToNextWave()
    {
        _currentWaveIndex++;
        StartCurrentWave();
    }

    private void OnLevelCompleted()
    {
        // TODO: UI, rewards, unlock next level, etc.
        StartNextLevel();
    }

    public void StartNextLevel()
    {
        StartLevel(++_currentLevelIndex);
    }
}
