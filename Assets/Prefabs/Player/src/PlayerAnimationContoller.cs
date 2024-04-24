using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Animations;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Unity.VisualScripting;
using Mono.Cecil.Rocks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerAnimationContoller : CharacterAnimationController
{
    #if UNITY_EDITOR
    //assignments show or not
    [SerializeField] private bool _assignmentsShow = false, _debug, _procedurallAssignmentsShow = false, 
        _rotateDirectionLog;
    #endif
    #region Assignment
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMovement _playerMovment;
    [SerializeField] private PlayerPhysic _playerPhysic;
    [SerializeField] private PlayerLook _playerLook;
    private CapsuleCollider _capsuleCollider;
    private Rigidbody _rigidbody;
    private InputControls _input;
    //private new LayerMask groundLayer;
    #endregion
    private Vector2 _smootedrotateDirection, rotateDirection;
    #region Variables
    [Range(0.0f, 10f)]
    [SerializeField] private float _angleSmoothTime, _upwardValue = 1f;
    [SerializeField] private bool _slopeCheck = true;
    private bool isLeftFootUp, isRightFootUp;
    #endregion
    #region  Procedurall animation variabels
    private bool _leftFootisUp,_rightFootIsUp;
    [Range(0.0f, 1f)] [SerializeField] private float _footIKWeight = 1;
    [Range(0.0f, 20f)] [SerializeField] private float _footIKRotationSmootTime = 20f, _footIKPositionYAxisSmootTime, 
        _bodyIKSmootTime = 10f;
    [Range(0.0f, 1f)] [SerializeField] private float _slopeStepHight = 0.2f;
    private quaternion _lastLeftFootRotation,_lastRightFootRotation;
    private float _lastLeftFootPositionYAxis,_lastRightFootPositionYAxis, _lastBodyPositionYAxis;
    [SerializeField] private bool _footIKRotation;
    [SerializeField] private Transform _leftToes, _rightToes;
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerMovment = GetComponent<PlayerMovement>();
        _playerPhysic = GetComponent<PlayerPhysic>();
        _playerLook = GetComponent<PlayerLook>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _input = new InputControls();
        _input.Player.Enable();

        groundLayer = _playerPhysic.groundLayer;
    }

    private void FixedUpdate()
    {
        AnimatorProcess();

        #if UNITY_EDITOR
        DebugLogProcess();
        #endif
        
    }

    private void AnimatorProcess(){
        float normalizedSpeed = Calculate.Normalize(0 , _playerMovment.sprintSpeed, _playerMovment.currentMoveSpeed);
        _animator.SetFloat("speed",_playerMovment.currentMoveSpeed);
        //get difference of character and input in movment
        float angleDifference = _playerMovment.CharacterDirectionDifference();
        //calculate direction for blend tree
        rotateDirection = Calculate.AngleToVector2(angleDifference);
        _smootedrotateDirection = SmootDirection(_smootedrotateDirection,rotateDirection);
        //move angle
        switch(_playerLook.cameraMode){
            case PlayerLook.CameraMode.FirstPerson:
                _animator.SetFloat("Velocity x",rotateDirection.x);
                _animator.SetFloat("Velocity z",rotateDirection.y);        
                break;
            case PlayerLook.CameraMode.ThirdPerson:
                    _animator.SetFloat("Velocity x",_smootedrotateDirection.y);
                    _animator.SetFloat("Velocity z",_smootedrotateDirection.x);
                break;
        }
        //moving angle: geting from input
        if(IsMoving()) {
            _animator.SetBool("IsMoving",true);
            DisableRootMotion();
            _animator.SetBool("isRightFootUp", isRightFootUp);
        } 
        else _animator.SetBool("IsMoving",false);
            
        //move speed
        //_animator.SetFloat("speed",normalizedSpeed);

        //Jump
        if(_playerPhysic.GroundCheck()) _animator.SetBool("IsGrounded",true); else {_animator.SetBool("IsGrounded",false);}

        // Forward moving
        _animator.SetBool("IsMovingToForward",IsMovingToForward());

        // character front surface is upward or downward
        float frontSurfaceHeight = _playerPhysic.FrontSurfaceHeight();
        // downward
        if(frontSurfaceHeight >= 0.1){
            _upwardValue =  Mathf.Lerp(_upwardValue, 2, (_bodyIKSmootTime + 1.5f)  * Time.fixedDeltaTime);
            _animator.SetFloat("upwardValue", _upwardValue);
        // upward
        }else if(frontSurfaceHeight <= -0.1){
            _upwardValue =  Mathf.Lerp(_upwardValue, 0, (_bodyIKSmootTime + 1.5f) * Time.fixedDeltaTime);
            _animator.SetFloat("upwardValue", _upwardValue);
        }
        // Direct
        else {
            _upwardValue =  Mathf.Lerp(_upwardValue, 1, (_bodyIKSmootTime + 1.5f) * Time.fixedDeltaTime);
            _animator.SetFloat("upwardValue", _upwardValue);
        }
    }

    public bool IsMoving(){
        return _input.Player.Movment.IsPressed();
    }
    public void EnableRootMotion(){
        _animator.applyRootMotion = true;
    }
    public void DisableRootMotion(){
        _animator.applyRootMotion = false;
    }

    private bool IsMovingToForward(){
        float differnceAngle =  _playerMovment.CharacterDirectionDifference();
        if(differnceAngle <= 60 && differnceAngle >= -60) return true;
        else return false;
    }

    ///<summary>controll direction change speed</summary>
    private Vector2 SmootDirection(Vector2 currentDirection,Vector2 newDirection){
        //set X
        float x = Mathf.Lerp(currentDirection.x, newDirection.x, _angleSmoothTime * Time.fixedDeltaTime);
        //set Y
        float y = Mathf.Lerp(currentDirection.y, newDirection.y, _angleSmoothTime * Time.fixedDeltaTime);
        return new Vector2(x,y);
    }
    
    /// <summary>
    /// check for target foot is Up or not
    /// </summary>
    private bool IsFootUp(AvatarIKGoal avatarIKGoal){
        if(avatarIKGoal == AvatarIKGoal.LeftFoot) return _leftFootisUp;
        else if(avatarIKGoal == AvatarIKGoal.RightFoot) return _rightFootIsUp;
        else return false;
    }

    public void IsLeftFootUp(){
        isLeftFootUp = true;
        isRightFootUp = false;
    }
    public void IsRightFootUp(){
        isRightFootUp = true;
        isLeftFootUp = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
       if (_animator != null && _playerPhysic.GroundCheck())
        {
            WhichFootIsAhead();

            SetBodyDistanceFromGround();

            // Set the position and rotation of the left foot
            SetFootIK(AvatarIKGoal.LeftFoot);
            
            // Set the position and rotation of the right foot
            SetFootIK(AvatarIKGoal.RightFoot);
        }
    }

    void OnAnimatorMove()
    {
        if (_animator != null)
        {
            if(_animator.applyRootMotion){
                _rigidbody.velocity = _animator.velocity;
                _rigidbody.AddForce(Physics.gravity * _rigidbody.mass);
            }
        }
    }

    private void SetFootIK(AvatarIKGoal foot)
    {
        Vector3 footPosition = _animator.GetIKPosition(foot);

        Quaternion footRotation = _animator.GetIKRotation(foot);

        //the default Y axis of character in flat ground
        float ballanceYaxis = transform.position.y;

        // Raycast to find the ground beneath the foot
        RaycastHit groundHit;
        if (Physics.Raycast(footPosition + Vector3.up * 0.5f, Vector3.down, out groundHit, Mathf.Infinity, groundLayer))
        {
            float yAxisDifference = groundHit.point.y - ballanceYaxis;
            Vector3 correctedPosition;

            //Calculate the foot rotation based on the ground normal
            Quaternion groundHitRotation = Quaternion.LookRotation(Vector3.Cross(transform.right, groundHit.normal), groundHit.normal);
            Quaternion corecctedRotation;
            Quaternion nextRotation;
            float nextYAxis;
            // Left foot
            if(foot == AvatarIKGoal.LeftFoot){
                nextYAxis = Mathf.Lerp(_lastLeftFootPositionYAxis, yAxisDifference, _footIKPositionYAxisSmootTime * Time.fixedDeltaTime);
                correctedPosition = footPosition + Vector3.up * nextYAxis;
                _lastLeftFootPositionYAxis = nextYAxis;
                
                // Calculate the rotation to align with the ground normal
                corecctedRotation = FootAndGroundRotation(footRotation,groundHitRotation);
                nextRotation = Quaternion.Lerp(_lastLeftFootRotation, corecctedRotation, _footIKRotationSmootTime * Time.fixedDeltaTime);
                _lastLeftFootRotation = nextRotation;

            // Right foot
            }else{
                nextYAxis = Mathf.Lerp(_lastRightFootPositionYAxis, yAxisDifference, _footIKPositionYAxisSmootTime * Time.fixedDeltaTime);
                correctedPosition = footPosition + Vector3.up * nextYAxis;
                _lastRightFootPositionYAxis = nextYAxis;
                
                // Calculate the rotation to align with the ground normal
                corecctedRotation = FootAndGroundRotation(footRotation,groundHitRotation);
                nextRotation = Quaternion.Lerp(_lastRightFootRotation, corecctedRotation, _footIKRotationSmootTime * Time.fixedDeltaTime);
                _lastRightFootRotation = nextRotation;
                
            }
            AvatarIKHint hint;
            if(foot == AvatarIKGoal.LeftFoot) hint = AvatarIKHint.LeftKnee;
            else hint = AvatarIKHint.RightKnee;
            Vector3 hintPosition = _animator.GetIKHintPosition(hint);
            Vector3 correctedHintPosition = hintPosition + Vector3.up * yAxisDifference;

            // Set the foot IK position and rotation
            _animator.SetIKPosition(foot, correctedPosition);
            _animator.SetIKHintPosition(hint, correctedHintPosition);
            _animator.SetIKRotation(foot, nextRotation);
            
            _animator.SetIKPositionWeight(foot, 1f);
            _animator.SetIKHintPositionWeight(hint, 1f);
            _animator.SetIKRotationWeight(foot, 1f);
        }
    }

    private float DistanceToGround (Vector3 position){
        RaycastHit groundHit;
        if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out groundHit, Mathf.Infinity, groundLayer)){
            return Vector3.Distance(position, groundHit.point);
        }
        return 0;
    }

    /// <summary>
    /// Calculation of the rotation of the foot along with the rotation of the ground surface
    /// </summary>
    private Quaternion FootAndGroundRotation(Quaternion footRotation, Quaternion groundHitRotation){
        Vector3 footEulerAngles = footRotation.eulerAngles;
        Vector3 groundEulerAngles = groundHitRotation.eulerAngles;
        return Quaternion.Euler(groundEulerAngles.x + footEulerAngles.x,footEulerAngles.y,groundEulerAngles.z + footEulerAngles.z);
    }

    private void SetBodyDistanceFromGround(){
        // calculate SmootTime
        float currentSmootTime;
        if(IsMoving()){
            float percentOfSpeedChange = Calculate.ChangePercent(_playerMovment.walkSpeed,_playerMovment.currentMoveSpeed);
            currentSmootTime = Calculate.IncreaseByPercent(_bodyIKSmootTime, percentOfSpeedChange);
        }
        else currentSmootTime = _bodyIKSmootTime;
        // set distance
        RaycastHit groundHit;
        if(Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out groundHit, Mathf.Infinity, groundLayer)){
            float difference = groundHit.point.y - transform.position.y;
            float nexYAxis = Mathf.Lerp(_lastBodyPositionYAxis, difference, currentSmootTime * Time.fixedDeltaTime);
            _animator.bodyPosition += Vector3.up * nexYAxis;
            _lastBodyPositionYAxis = nexYAxis;
        }
    }

    private void WhichFootIsAhead(){
        // Get the position of the left and right foot in world space
        Vector3 leftFootPosition = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        Vector3 rightFootPosition = _animator.GetIKPosition(AvatarIKGoal.RightFoot);

        // Convert foot positions to local space relative to the character
        Vector3 localLeftFootPosition = transform.InverseTransformPoint(leftFootPosition);
        Vector3 localRightFootPosition = transform.InverseTransformPoint(rightFootPosition);

        // Compare their local z values to determine which is further forward
        if (localLeftFootPosition.z > localRightFootPosition.z)
        {
            IsLeftFootUp();
        }
        else
        {
            IsRightFootUp();
        }
    }

    void OnDrawGizmos(){
        //Vector3 frontPoint = transform.position + transform.forward * frontOffset;
        
        // RaycastHit bodyHit;
        // if (Physics.Raycast(transform.position + Vector3.up * 0.5f , Vector3.down, out bodyHit, Mathf.Infinity, groundLayer));
        // Gizmos.DrawSphere(bodyHit.point, 0.1f);
        // Debug.Log(bodyHit.point.y);
    }

    #if UNITY_EDITOR
    ///<summary>logs</summary>
    void DebugLogProcess(){
        if(_debug){
            if(_rotateDirectionLog) Debug.Log($"Rotate direction {rotateDirection}");
        }
    }
    #endif
    #if UNITY_EDITOR
    [CustomEditor(typeof(PlayerAnimationContoller))]
    [CanEditMultipleObjects]
    private class PlayerAnimationContollerController: Editor{

        public override void OnInspectorGUI()
        {
            Color originalColor = GUI.contentColor;

            PlayerAnimationContoller playerAnimationContoller = (PlayerAnimationContoller)target;

            serializedObject.Update();
            
            //make assign value in foldout
            GUI.contentColor = Color.yellow;
            playerAnimationContoller._assignmentsShow = EditorGUILayout.Foldout(playerAnimationContoller._assignmentsShow,"Assignments");
            if(playerAnimationContoller._assignmentsShow){
                GUI.contentColor = originalColor;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._animator)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._playerMovment)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._playerLook)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._playerPhysic)));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Variabels");
            GUI.contentColor = originalColor;
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._angleSmoothTime)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._slopeStepHight)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._slopeCheck)));
            EditorGUILayout.Space();

            //make procedurall value in foldout
            GUI.contentColor = Color.yellow;
            playerAnimationContoller._procedurallAssignmentsShow = 
            EditorGUILayout.Foldout(playerAnimationContoller._procedurallAssignmentsShow,"Procedurall animations");
            if(playerAnimationContoller._procedurallAssignmentsShow){
                GUI.contentColor = originalColor;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._footIKRotationSmootTime)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._footIKPositionYAxisSmootTime)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._bodyIKSmootTime)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._footIKRotation)));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._leftToes)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._rightToes)));
            }

            EditorGUILayout.Space();
                GUI.contentColor = Color.yellow;
                //make debug value in foldout
                playerAnimationContoller._debug = EditorGUILayout.Foldout(playerAnimationContoller._debug,"Debug log");
                if(playerAnimationContoller._debug){
                    GUI.contentColor = originalColor;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._rotateDirectionLog)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerAnimationContoller._footIKWeight)));
                }

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}