using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerUIController : MonoBehaviour
{
    #if UNITY_EDITOR
    //assignments show or not
    [SerializeField] private bool _assignMentsShow = false;
    #endif
    #region
    [SerializeField] private Slider _jumpForceBar;
    [SerializeField] private PlayerMovement _playerMovement;
    #endregion

    void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _jumpForceBar.maxValue = _playerMovement.maximumJumpForce;
        _jumpForceBar.minValue = _playerMovement.minimumJumpForce;
    }

    void FixedUpdate()
    {
        _jumpForceBar.value = _playerMovement.jumpForce;
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(PlayerUIController))]
    [CanEditMultipleObjects]
    private class PlayerUIContrilerEditorController: Editor{
        public override void OnInspectorGUI()
        {
            PlayerUIController playerUIController = (PlayerUIController)target;

            serializedObject.Update();
            
            //make assign value in foldout
            playerUIController._assignMentsShow = EditorGUILayout.Foldout(playerUIController._assignMentsShow,"Assignments");
            if(playerUIController._assignMentsShow){
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(playerUIController._jumpForceBar)));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
