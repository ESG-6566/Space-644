using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ybot : MonoBehaviour
{
    Animator animator;
    InputControls _input; 
    // Start is called before the first frame update
    void Start()
    {
        //The inputControler must assign in awak or start
        _input = new InputControls();
        _input.Player.Enable();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_input.Player.H.IsPressed()){
            Debug.Log("h is pressed");
            animator.Play("H");
        }
    }
}
