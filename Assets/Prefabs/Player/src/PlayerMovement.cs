using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerMovement : CharacterMovement
{
    #if UNITY_EDITOR
    //assignments show or not
    [SerializeField] private bool _assignMentsShow = false;
    [SerializeField] private bool _debug;
    [SerializeField] private bool _inputAndPlayerAngleDifference;
    [SerializeField] private bool _moveSpeedLog;
    #endif
    #region Variables
    private Rigidbody _rb;
    private InputControls _input;
    private PlayerLook _playerLook;
    [SerializeField] private PlayerPhysic _playerPhysic;
    [SerializeField] public float minimumJumpForce = 100;
    [SerializeField] public float jumpForce = 0;
    [SerializeField] public float maximumJumpForce = 600;
    [SerializeField] private float _jumpForceSmoothTime = 5;
    private bool readForJump = false;
    public float currentMoveSpeed;
    [Range(0.0f, 500f)] [SerializeField] public float walkSpeed = 50, sprintSpeed = 100, runSpeed = 200;
    [Range(0.0f, 100f)][SerializeField] private float _rotationSpeedReducerPercent = 10,
        _upwardSpeedReducerPercent = 10;
    
    [Range(0.0f, 100f)]
    [SerializeField] private float _rotationSmoothTime = 5;
    private float _turnSmoothVelocity;
    [SerializeField] public float speedSmoothTime = 2;
    private GameObject _mainCamera;
    [SerializeField] private Animator _animator;
    public float targetAngle;
    public float angle;
    [SerializeField] public bool rotation;
    public bool isJumping;
    #endregion
    
    private void Awake()
    {
        _input = new InputControls();
        _input.Player.Enable();
        //The inputControler must assign in awak or start
        _input.Player.Jump.started += context => {readForJump = true;};
        _input.Player.Jump.canceled += Jump;

        _rb = GetComponent<Rigidbody>();
        _playerLook = GetComponent<PlayerLook>();
        _playerPhysic = GetComponent<PlayerPhysic>();
        _animator = GetComponent<Animator>();

        jumpForce = minimumJumpForce;

        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    void Update()
    {

        switch(_playerLook.cameraMode){
            case PlayerLook.CameraMode.FirstPerson:
                //FirstPersonMove();
                break;
            case PlayerLook.CameraMode.ThirdPerson:
                ThirdPrsonMove();
                break;
        }

        CheckAndSetMovmentSpeed();
        
        ChargeJumpForce();

        // if(_input.Player.H.IsPressed()){
        //     _animator.Play("Walk rooted");
        // }
        if(_input.Player.H.IsPressed()){
            _animator.Play("Walk 1");
        }
        // if(_input.Player.R.IsPressed()){
        //     _animator.Play("rtl");
        // }

        #if UNITY_EDITOR
        DebugLogProcess();
        #endif
    }
    
    #if UNITY_EDITOR
    ///<summary>logs</summary>
    void DebugLogProcess(){
        if(_debug){
            if(_inputAndPlayerAngleDifference) Debug.Log("Angle Difference: " + CharacterDirectionDifference());
            if(_moveSpeedLog) Debug.Log("Move speed: " + currentMoveSpeed);
        }
    }
    #endif
    /// <summary>
    /// Calculate reduced speed
    /// </summary>
    
    private void CheckAndSetMovmentSpeed(){
        if(_input.Player.Sprint.IsPressed()) moveingSpeedState = MoveingSpeedState.Sprint;
        else moveingSpeedState = MoveingSpeedState.Run;

        
        float targetSpeed;
        float decreasePercent = 0;
        
        //check player move key is pressed or not
        if(_input.Player.Movment.IsPressed()) {

            //set move speed gradually to run or walk
            if(isSprinting) targetSpeed = sprintSpeed;
            else targetSpeed = runSpeed;
            
            // check for rotation
            if(IsRotaiting()) decreasePercent += _rotationSpeedReducerPercent;
            // check for upward move
            if(_playerPhysic.movementSlopeStatus == MovementSlopeStatus.Upward) decreasePercent += _upwardSpeedReducerPercent;
        }
        else {
            targetSpeed = 0f;
            decreasePercent = 0;
            isSprinting = false;
        }

        // Apply decrease 
        targetSpeed = Calculate.DecreaseByPercent(targetSpeed, decreasePercent);
        currentMoveSpeed = (float)Math.Round(Mathf.Lerp(currentMoveSpeed, targetSpeed, speedSmoothTime * Time.fixedDeltaTime), 4);
    }

    /// <summary>
    /// Check when player is indirect moving
    /// </summary>
    private bool IsRotaiting(){
        //get direction difference to effecting on move speed in rotation
        float directionDifference = CharacterDirectionDifference();
        if(directionDifference >= 5 || directionDifference <= -5) return true;
        else return false;
    }

    private void ChargeJumpForce(){
        if (readForJump && _playerPhysic.GroundCheck() && _input.Player.Jump.IsPressed())
        {
            jumpForce = Mathf.Lerp(jumpForce, maximumJumpForce, _jumpForceSmoothTime * Time.fixedDeltaTime);
        }
        else jumpForce = minimumJumpForce;

    }
    private void Jump(InputAction.CallbackContext context){
        //Debug.Log("canceled");
        if(_playerPhysic.GroundCheck() && readForJump){
            //_rb.velocity = Vector3.up * _jumpForce * Time.fixedDeltaTime;
            //_rb.AddForce(Vector3.up * jumpForce* Time.fixedDeltaTime, ForceMode.Impulse);
            readForJump = false;
            jumpForce = minimumJumpForce;

        }
        else jumpForce = minimumJumpForce;
        if(_input.Player.Jump.IsPressed()){
            //animation
            _animator.SetTrigger("Jump");
        }
    }
    

    public void FirstPersonMove(){
        float targetAngle;
        Vector2 _inputAxis = _input.Player.Movment.ReadValue<Vector2>().normalized;
        
        if(_inputAxis == new Vector2(-0.71f,0.71f).normalized || _inputAxis == new Vector2(0.71f,0.71f).normalized){
            targetAngle = Mathf.Atan2(_inputAxis.x,_inputAxis.y) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _rotationSmoothTime * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0,angle,0);
        }
        else{
            //rotate character to camera look angle
            targetAngle = _mainCamera.transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0,targetAngle,0);
        }
        //move to input angle
        float speedAndTime = currentMoveSpeed * Time.fixedDeltaTime;
        Vector3 moveDirection = transform.right * _inputAxis.x + transform.forward * _inputAxis.y;
        _rb.velocity = new Vector3(moveDirection.x * speedAndTime, _rb.velocity.y, moveDirection.z * speedAndTime);
       
    }
    
    private void ThirdPrsonMove(){
        if(_input.Player.Movment.IsPressed()){
            Vector2 _inputAxis = _input.Player.Movment.ReadValue<Vector2>().normalized;
            // Rotation
            targetAngle = Mathf.Atan2(_inputAxis.x,_inputAxis.y) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _rotationSmoothTime * Time.fixedDeltaTime);
            if(rotation) {
                transform.rotation = Quaternion.Euler(0,angle,0);
            }

            // Move
            float speedAndTime = currentMoveSpeed * Time.fixedDeltaTime;
            Vector3 moveDirection = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;
            //_rb.velocity = new Vector3(moveDirection.x * speedAndTime, _rb.velocity.y, moveDirection.z * speedAndTime);
        }
    }

    ///<summary>the diference between input angle and character angle</summary>
    public float CharacterDirectionDifference(){
        float characterAngle = transform.eulerAngles.y;

        // Calculate the difference between the angles
        float difference = Mathf.Repeat(characterAngle - targetAngle + 180, 360) - 180;

        return difference;
    }

    public void StartJumping(){
        isJumping = true;
        rotation = false;
    }
    public void finishJumping(){
        isJumping = false;
        rotation = true;
    }
    public void EnableRotation(){
        rotation = true;
    }
    public void DisableRotation(){
        rotation = false;
    }

	#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 endPoint = transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward * 2f;
        Gizmos.DrawLine(transform.position, endPoint);

        Gizmos.color = Color.green;        
        endPoint = transform.position + Quaternion.Euler(0, targetAngle, 0) * Vector3.forward * 2f;
        Gizmos.DrawLine(transform.position, endPoint);
    }

    [CustomEditor(typeof(PlayerMovement))]
    [CanEditMultipleObjects]
    private class PlayerMovementEditorController: Editor{
        public override void OnInspectorGUI()
        {
            PlayerMovement playerMovement = (PlayerMovement)target;
            Color originalColor = GUI.contentColor;

            serializedObject.Update();
            
            //make assign value in foldout
            GUI.contentColor = Color.yellow;
            playerMovement._assignMentsShow = EditorGUILayout.Foldout(playerMovement._assignMentsShow,"Assignments");
            if(playerMovement._assignMentsShow){
                GUI.contentColor = originalColor;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._playerPhysic)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._animator)));
            }

            GUI.contentColor = originalColor;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.maximumJumpForce)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.minimumJumpForce)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._jumpForceSmoothTime)));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._rotationSmoothTime)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.speedSmoothTime)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.walkSpeed)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.runSpeed)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.sprintSpeed)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._rotationSpeedReducerPercent)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._upwardSpeedReducerPercent)));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement.rotation)));

            EditorGUILayout.Space();

            //make debug value in foldout
            GUI.contentColor = Color.yellow;
            playerMovement._debug = EditorGUILayout.Foldout(playerMovement._debug,"Debug log");
            if(playerMovement._debug){
                GUI.contentColor = originalColor;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._inputAndPlayerAngleDifference)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerMovement._moveSpeedLog)));
            } 

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}