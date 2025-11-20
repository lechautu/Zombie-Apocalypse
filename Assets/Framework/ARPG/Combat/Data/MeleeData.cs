// ARPG/Combat/Data/MeleeData.cs
using UnityEngine;

namespace ARPG.Combat
{
    [CreateAssetMenu(fileName = "MeleeData", menuName = "ARPG/Combat/Melee Data")]
    public class MeleeData : ScriptableObject
    {
        public DamageSpec damage = new();
        public HitShapeBox shape = new();
    }
}
