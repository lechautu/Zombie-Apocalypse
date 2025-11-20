using ARPG.Core;
using UnityEngine;

namespace ARPG.Combat
{
    public interface ISkillPostEffect
    {
        void PostEffect(HitInfo hitInfo);
    }
}