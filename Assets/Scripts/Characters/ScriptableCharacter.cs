using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "ScriptableObjects/Character")]
    public class ScriptableCharacter : ScriptableObject
    {
        public int maxHealth = 100;

        [Header("For Enemy AI")]
        public float speed = 5f;
    }
}
