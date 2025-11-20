using UnityEngine;

public class JumpStartGate : StateMachineBehaviour
{
    [Range(0f, 1f)] public float minCrouchNormalizedTime = 0.2f;

    // OnStateEnter: set the lock
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("JumpStartLock", true);
    }

    // OnStateUpdate: release the lock after min crouch time
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= minCrouchNormalizedTime)
            animator.SetBool("JumpStartLock", false);
    }

    // Safety: clear the lock on exit
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("JumpStartLock", false);
    }
}
