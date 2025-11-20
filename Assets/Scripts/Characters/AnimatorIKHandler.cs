using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.Animation
{
    public class AnimatorIKHandler : MonoBehaviour
    {
        [SerializeField] TwoBoneIKConstraint _leftArmConstraint;
        [SerializeField] RigBuilder rigBuilder;
        
        public void SetIKPosition(Transform LHandTarget, Transform LElbowHint)
        {
            var data = _leftArmConstraint.data;
            data.target = LHandTarget;
            data.hint = LElbowHint;
            _leftArmConstraint.data = data;
            
            rigBuilder.Build();
        }
    }
}
