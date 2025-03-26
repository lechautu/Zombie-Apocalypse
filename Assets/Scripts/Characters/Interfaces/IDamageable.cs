using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}
