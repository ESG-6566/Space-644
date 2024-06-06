using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public bool isSprinting, isRunning;
    public MoveingSpeedState moveingSpeedState;
}
public enum MoveingSpeedState{
    Run,
    Sprint,
    Walk
}