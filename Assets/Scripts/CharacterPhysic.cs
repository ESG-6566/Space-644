using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysic : MonoBehaviour
{
    public LayerMask groundLayer;
    public MovementSlopeStatus movementSlopeStatus = MovementSlopeStatus.Flat;

    /// <summary>
    /// Calculate character front surface hight
    /// </summary>
    public float FrontSurfaceHeight(float frontOffset = 0.5f){
        Vector3 frontPoint = transform.position + transform.forward * frontOffset;
        RaycastHit bodyHit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f , Vector3.down, out bodyHit, Mathf.Infinity, groundLayer));
        RaycastHit hit;
        // Perform the raycast downward from the front point
        if (Physics.Raycast(frontPoint + Vector3.up * 0.5f , Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            if(hit.point.y - bodyHit.point.y >= 0.1f || hit.point.y - bodyHit.point.y <= -0.1f){
                return hit.point.y - bodyHit.point.y;
            }else return 0f;
        }
        else return 0f;
    }

    // character front surface is upward or downward
    public MovementSlopeStatus GetMovementSlopeStatus(){
        float frontSurfaceHeight = FrontSurfaceHeight();
        // upward
        if(frontSurfaceHeight >= 0.1){
            return MovementSlopeStatus.Upward;
        // downward
        }else if(frontSurfaceHeight <= -0.1){
            return MovementSlopeStatus.Downward;
        }
        // Direct
        else {
            return MovementSlopeStatus.Flat;
        }
    }
}

public enum MovementSlopeStatus{
    Upward,
    Downward,
    Flat
}