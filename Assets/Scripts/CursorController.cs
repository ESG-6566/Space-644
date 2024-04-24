using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private InputControls _input;
    [SerializeField] private CinemachineFreeLook freeLookCamera;

    // Start is called before the first frame update
    void Start()
    {
        _input = new InputControls();
        _input.Player.Enable();
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(_input.Player.PauseGame.IsPressed()){
        //     if(freeLookCamera.enabled == true) freeLookCamera.enabled = false;
        //     else freeLookCamera.enabled = true;
        // }
    }
}
