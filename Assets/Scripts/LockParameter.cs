using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockParameter : StateMachineBehaviour
{
    [SerializeField] private bool doLockMirror = true, doUnlockMirror = true,
        doLocValue = true, doUnlockValue = true;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(doLockMirror)
        animator.gameObject.GetComponent<CharacterAnimationController>().isStateMirrorLock = true;

        if(doLocValue)
        animator.gameObject.GetComponent<CharacterAnimationController>().isStateValueLock = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(doUnlockMirror)
        animator.gameObject.GetComponent<CharacterAnimationController>().isStateMirrorLock = false;
        
        if(doUnlockValue)
        animator.gameObject.GetComponent<CharacterAnimationController>().isStateValueLock = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
