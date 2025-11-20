using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "New Character", menuName = "Game/Data/Character")]
    public class ScriptableCharacter : ScriptableObject
    {
        public int maxHealth = 100;

        [Header("For Enemy AI")]
        public float speed = 5f;
        public int score;
    }
}
