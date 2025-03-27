using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Animation
{
    public class AnimatorIKHandler : MonoBehaviour
    {
        private Animator animator;

        [Header("Right Hand IK")]
        public Transform rightHandTarget;  // Correct hand position on gun
        public Transform rightElbowHint;   // Helps with elbow direction

        [Header("Left Hand IK (for rifles)")]
        public Transform leftHandTarget;   // Left hand grip position
        public Transform leftElbowHint;    // Helps with elbow direction

        [Header("Body Adjustments")]
        public Transform spineTarget; // Adjust spine manually
        public Transform spineBone;   // Assign the spine bone from the character rig
        public float spineRotationSpeed = 5f; // Adjust rotation smoothing

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (rightHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            if (rightElbowHint != null)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowHint.position);
            }

            if (leftHandTarget != null) // For two-handed weapons
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            if (leftElbowHint != null)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowHint.position);
            }
        }

        void Update()
        {
            if (spineBone != null && spineTarget != null)
            {
                // Smoothly rotate the spine to match the aim target
                Quaternion targetRotation = Quaternion.LookRotation(spineTarget.forward, transform.up);
                spineBone.rotation = Quaternion.Slerp(spineBone.rotation, targetRotation, Time.deltaTime * spineRotationSpeed);
            }
        }
    }
}
