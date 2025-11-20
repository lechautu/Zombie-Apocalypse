// ARPG/Combat/Data/DamageSpec.cs
using UnityEngine;

namespace ARPG.Combat
{
    [System.Serializable]
    public class DamageSpec
    {
        public string attackId = "";
        public float damage = 10f;
        public float hitstun = 0.2f;
        public float hitlag = 0.05f;
        public float knockbackForce = 6f;
    }
}
