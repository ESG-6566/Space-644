using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{   
    public LayerMask groundLayer;
    public bool isStateMirrorLock = false;
    public bool isStateValueLock = false;
    public InputControls _input;
    [SerializeField] public Animator _animator;
    public void Jump(){
        if(_input.Player.Jump.IsPressed()){
            //animation
            _animator.SetBool("jump", true);
        }else _animator.SetBool("jump", false);
    }
    
}
