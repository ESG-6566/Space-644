using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerPhysic : CharacterPhysic
{
    #if UNITY_EDITOR
    [SerializeField] private bool _debug;
    [SerializeField] private bool _groundCheckLog,_velocityLog,_velocityMagnitude, _slopeLog;
    #endif
    #region Variables
    private Rigidbody _rb;
    private ConstantForce _constantForce;
    private InputControls _input;
    //private CapsuleCollider _capsuleCollider;
    public float slope;
    public Vector3 raycastOrigin;
    private CharacterController _characterController;
    [SerializeField] public Transform leftFoot,rightFoot;
    #endregion
    #region get and set
    //public LayerMask groundLayer {get { return _groundLayer; }}
    #endregion
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _constantForce = GetComponent<ConstantForce>();
        _characterController = GetComponent<CharacterController>();
        _input = new InputControls();
        _input.Player.Enable();
    }

    private void FixedUpdate()
    {
        //dont pull down character if is grounded
        // if(GroundCheck()){
        //     _rb.isKinematic = true;
        // }
        // else{
        //     _rb.isKinematic = false;
        // }

        movementSlopeStatus = GetMovementSlopeStatus();
        
        //logs
        #if UNITY_EDITOR
        DebugLogProcess();
        #endif
    }

    #if UNITY_EDITOR
    ///<summary>logs</summary>
    void DebugLogProcess(){
        if(_debug){
            if(_groundCheckLog) Debug.Log($"Groundcheck {GroundCheck()}");
            if(_velocityLog) Debug.Log($"Velocity {_rb.velocity}");
            if(_velocityMagnitude) ConsoleLoger.velocityMagnitude = _rb.velocity.magnitude;
            if(_slopeLog) ConsoleLoger.slope = slope;
        }
    }
    #endif

    /// <summary>
    /// Are two foots on the ground?
    /// </summary>
    public bool AreFootsOnGround(){
        //is left foot on ground?
        if(!Physics.Raycast(leftFoot.position, Vector3.down, 0.3f, groundLayer)) return false;
        //is right foot on ground?
        else if(!Physics.Raycast(rightFoot.position, Vector3.down, 0.3f, groundLayer)) return false;
        else return true;
    }

    public bool IsLeftFootOnGround(){
        //is left foot on ground?
        return Physics.Raycast(leftFoot.position, Vector3.down, 0.3f, groundLayer);
    }
    public bool IsRightFootOnGround(){
        //is left foot on ground?
        return Physics.Raycast(rightFoot.position, Vector3.down, 0.3f, groundLayer);
    }

    ///<summary>check is player on ground or not</summary>
    public bool GroundCheck(float sensitivity = 0.3f){
        //is left foot on ground?
        //if(Physics.Raycast(leftFoot.position, Vector3.down, 0.3f, groundLayer)) return true;
        //is right foot on ground?
        //if(Physics.Raycast(rightFoot.position, Vector3.down, 0.3f, groundLayer)) return true;
        
        Vector3 raycastDirection = Vector3.down;

        float raycastDistance = 0.5f + sensitivity;

        return Physics.Raycast(transform.position + Vector3.up * 0.5f, raycastDirection, raycastDistance, groundLayer);
    }

    private void StopMoving(){
        _rb.velocity = Vector3.zero;
    }

    public void DisableCollider(){
        _characterController.enabled = false;
    }
    public void EnableCollider(){
        _characterController.enabled = true;
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(PlayerPhysic))]
    [CanEditMultipleObjects]
    private class PlayerPhysicEditorController: Editor{

        //assignments show or not
        [SerializeField] private bool assignMentsShow = false;
        public override void OnInspectorGUI()
        {
            Color originalColor = GUI.contentColor;
            PlayerPhysic playerPhysic = (PlayerPhysic)target;

            serializedObject.Update();
            
            //make assign value in foldout
            GUI.contentColor = Color.yellow;
            assignMentsShow = EditorGUILayout.Foldout(assignMentsShow,"Assignments");
            if(assignMentsShow){
                GUI.contentColor = originalColor;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic.groundLayer)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic.leftFoot)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic.rightFoot)));
            }

            GUI.contentColor = originalColor;
            

            EditorGUILayout.Space();

            //make debug value in foldout
            GUI.contentColor = Color.yellow;
            playerPhysic._debug = EditorGUILayout.Foldout(playerPhysic._debug,"Debug log");
            if(playerPhysic._debug){
                GUI.contentColor = originalColor;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic._groundCheckLog)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic._velocityLog)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic._velocityMagnitude)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerPhysic._slopeLog)));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
