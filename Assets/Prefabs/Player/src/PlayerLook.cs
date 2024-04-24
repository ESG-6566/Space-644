using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerLook : MonoBehaviour
{   
    #if UNITY_EDITOR
    //assignments show or not
    [SerializeField] private bool _assignMentsShow = false;
    #endif
    private InputControls _input;
    private float _cinemachineTargetPitch;
    private float _rotationVelocity;
    [SerializeField] public CameraMode cameraMode;
    [SerializeField] public GameObject cameraFollow;
    ///<summary>How far in degrees can you move the camera up</summary>
    [SerializeField] private float _firstPersonTopClamp = 90.0f;
    ///<summary>How far in degrees can you move the camera down</summary>
    [SerializeField] private float _firstPersonBottomClamp = -90.0f;
    ///<summary>How far in degrees can you move the camera up</summary>
    [SerializeField] private float _sensitivity = 5;
    [SerializeField] private CinemachineVirtualCamera _FPVirtualCamera;
    [SerializeField] private CinemachineFreeLook _TPFreeLook;
    [SerializeField] private GameObject _body;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _input = new InputControls();
        _input.Player.Enable();
        
        //switch to current view at start
        switch (cameraMode){
            case CameraMode.FirstPerson:
                SwitchToFirstPersonView();
                break;
            case CameraMode.ThirdPerson:
                SwitchToThirdPersonView();
                break;
        }

        AdjustCameraRotaition(_FPVirtualCamera);
    }

    private void Start()
    {
        _rotationVelocity = transform.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        switch (cameraMode){
            case CameraMode.FirstPerson:
                //FirstPersonCameraRotation();
                break;
            case CameraMode.ThirdPerson:
                //ThirdPersonCameraRotation();
                break;
        }
    }
    public enum CameraMode{
        FirstPerson,
        ThirdPerson
    }
    
    private void FirstPersonCameraRotation()
    {  
        Vector2 mousPosition = _input.Player.Look.ReadValue<Vector2>();
        _cinemachineTargetPitch += mousPosition.y * _sensitivity * Time.fixedDeltaTime * -1;
        _rotationVelocity += mousPosition.x * _sensitivity * Time.fixedDeltaTime;
    
        // clamp our pitch rotation
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _firstPersonBottomClamp, _firstPersonTopClamp);
        _rotationVelocity = ClampAngle(_rotationVelocity, -360f, 360f);

        // Update Cinemachine camera target pitch
        cameraFollow.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch,0f, 0f);

        // rotate the player left and right
        transform.rotation = Quaternion.Euler(0,_rotationVelocity,0);
    }

    private void ThirdPersonCameraRotation()
    {  
        Vector2 mousPosition = _input.Player.Look.ReadValue<Vector2>();
        _cinemachineTargetPitch += mousPosition.y * _sensitivity * Time.fixedDeltaTime;
        _rotationVelocity += mousPosition.x * _sensitivity * Time.fixedDeltaTime;

    
        // // clamp our pitch rotation
        // _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _thirdPersonBottomClamp, _thirdPersonTopClamp);
        // _rotationVelocity = ClampAngle(_rotationVelocity, -360f, 360f);

        // Cinemachine will follow this target
        cameraFollow.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch,_rotationVelocity, 0.0f);
        // CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
        //         _cinemachineTargetYaw, 0.0f);
    }

    ///<summary>Control camera Angle</summary>
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    ///<summary>Change camera view between FP and TP modes</summary>
    public void ChangeView(){
        switch (cameraMode){
            case CameraMode.FirstPerson:
                RecenterTPCamera();
                SwitchToThirdPersonView();
                cameraMode = CameraMode.ThirdPerson;
                break;
            case CameraMode.ThirdPerson:
                SwitchToFirstPersonView();
                cameraMode = CameraMode.FirstPerson;
                break;
        }
        
    }

    private void SwitchToFirstPersonView(){
        _FPVirtualCamera.Priority = 20;
        _TPFreeLook.Priority = 10;
    }
    private void SwitchToThirdPersonView(){
        _TPFreeLook.Priority = 20;
        _FPVirtualCamera.Priority = 10;
    }

    private void RecenterTPCamera(){
        _TPFreeLook.m_RecenterToTargetHeading.m_enabled = true;
        StartCoroutine(DisableRecenter());
        IEnumerator DisableRecenter()
        {
            yield return new WaitForSeconds(0.5f);
            _TPFreeLook.m_RecenterToTargetHeading.m_enabled = false;
        }
    }

    //adjust cinemachine rotation to player rotation angle
    private void AdjustCameraRotaition(CinemachineVirtualCamera cinemachine){
        CinemachinePOV cmvcPOV = cinemachine.GetCinemachineComponent<CinemachinePOV>();
        cmvcPOV.m_HorizontalAxis.Value = transform.rotation.eulerAngles.y;
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(PlayerLook))]
    [CanEditMultipleObjects]
    private class PlayerLookEditorController: Editor{

        public override void OnInspectorGUI()
        {
            PlayerLook playerLook = (PlayerLook)target;

            serializedObject.Update();
            
            //make assign value in foldout
            playerLook._assignMentsShow = EditorGUILayout.Foldout(playerLook._assignMentsShow,"Assignments");

            if(playerLook._assignMentsShow){
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook.cameraFollow)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook._FPVirtualCamera)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook._TPFreeLook)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook._body)));
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook.cameraMode)));

            switch (playerLook.cameraMode){
                case CameraMode.FirstPerson:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook._sensitivity)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook._firstPersonTopClamp)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerLook._firstPersonBottomClamp)));
                    break;
                case CameraMode.ThirdPerson:

                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
