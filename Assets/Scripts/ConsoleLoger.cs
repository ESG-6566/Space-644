using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is using to multy data console log
/// </summary>
public class ConsoleLoger : MonoBehaviour
{
    private static string logText = "";
    private static bool _velocityMagnitudeLog, _slopeLog;
    private static float _velocityMagnitude, _slope;
    private ConsoleLoger(){}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_velocityMagnitudeLog){
            logText += $", velocityMagnitude: {_velocityMagnitude}";
            _velocityMagnitudeLog = false;
        }
        if(_slopeLog){
            logText += $", Slope: {_slope}";
            _slopeLog = false;
        }

        //print log text in console
        if(logText != ""){
            Debug.Log(logText);
            logText = "";
        }
        //Debug.Log(velocityMagnitude);
    }
    public static float velocityMagnitude
    {
        set{
            _velocityMagnitude = value;
            _velocityMagnitudeLog = true;
        }
    }
    public static float slope
    {
        set{
            _slope = value;
            _slopeLog = true;
        }
    }
}
