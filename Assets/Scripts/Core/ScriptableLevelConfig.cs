using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    [System.Serializable]
    public struct WaveEntry
    {
        public EnemyType enemyType;
        public int amount;
    }

    [System.Serializable]
    public class WaveConfig
    {
        public List<WaveEntry> entries = new List<WaveEntry>();
    }

    [System.Serializable]
    public class LevelConfig
    {
        public List<WaveConfig> waves = new List<WaveConfig>();
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Data/Level Config")]
    public class ScriptableLevelConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>();

        public IReadOnlyList<LevelConfig> LevelConfigs => levelConfigs;

        public LevelConfig GetLevelConfig(int levelIndex)
        {
            // You can switch to index-based if you want (e.g. return levelConfigs[levelIndex])
            return levelConfigs[levelIndex];
        }

        public LevelConfig GetClampedLevelConfig(int levelIndex)
        {
            if (levelConfigs == null || levelConfigs.Count == 0)
                return null;

            int clamped = Mathf.Clamp(levelIndex, 0, levelConfigs.Count);
            return GetLevelConfig(clamped);
        }
    }
}
