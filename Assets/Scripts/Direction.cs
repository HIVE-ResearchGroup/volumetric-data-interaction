using System;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
    
public static class DirectionMethods
{
    public static Direction GetDirectionDegree(float angle)
    {
        var safeAngle = Mathf.Abs(angle) % 360.0f;

        switch (safeAngle)
        {
            case >= 315.0f or <= 45.0f:
                return Direction.Right;
            case >= 45.0f and <= 135.0f:
                return Direction.Up;
            case >= 135.0f and <= 225.0f:
                return Direction.Left;
            case >= 225.0f and <= 315.0f:
                return Direction.Down;
            default:
                Debug.LogError($"Angle cannot be converted to Direction! Value: {safeAngle} Check safeAngle conversion!");
                throw new ArgumentException("Angle cannot be converted to Direction!");
        }
    }

    public static Direction GetDirectionRad(float rad) => GetDirectionDegree(rad * Mathf.Rad2Deg);
}