using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Calculate
{
    public static float Normalize(float min, float max, float value)
    {
        // Ensure that min is less than max to avoid division by zero
        if (min >= max)
        {
            throw new ArgumentException("min must be less than max.");
        }

        // Clamp value to be within the range [min, max]
        value = Mathf.Clamp(value, min, max);

        // Normalize the value to the range [0, 1]
        float normalizedValue = (value - min) / (max - min);

        return normalizedValue;
    }

    ///<summary>Conver angle to Vector2</summary>
    public static Vector2 AngleToVector2(float angle){
        // Convert angle from degrees to radians
        angle = angle * Mathf.Deg2Rad;

        // Calculate the direction vector
       return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    /// <summary>
    /// Calculate the percentage change
    /// </summary>
    public static float ChangePercent(float a, float b){
        return (b - a) / a * 100f;
    }

    public static float IncreaseByPercent(float number, float percent){
        float increaseValue = number * (percent/100);
        return number + increaseValue;
    }

    public static float DecreaseByPercent(float current, float percent){
        float decreased = current * (100 - percent) / 100f;
        return decreased;
    }
}
