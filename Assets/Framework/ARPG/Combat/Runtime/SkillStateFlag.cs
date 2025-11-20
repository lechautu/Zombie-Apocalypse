using UnityEngine;

namespace ARPG.Combat
{
    public class SkillStateFlag : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.GetComponent<SkillController>()?.NotifySkillEnter();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.GetComponent<SkillController>()?.NotifySkillExit();
        }
    }
}
