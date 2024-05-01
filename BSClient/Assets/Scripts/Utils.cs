using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static float ConvertSwipeDistanceToThrowPower(float swipeDistance)
    {
        float maxSwipeDistance = PlayerController.Instance.GetMaxSwipeDistance();
        float maxThrowPower = PlayerController.Instance.GetThrowPowerRange();
        float lowestThrowPowerThreshold = PlayerController.Instance.GetLowestThrowPower();
        return swipeDistance / maxSwipeDistance * maxThrowPower + lowestThrowPowerThreshold;
    }

    public static float ConvertThrowPowerToSwipeDistance(float throwPower, float maxSwipeDistance, float maxThrowPower)
    {
        return throwPower / maxThrowPower * maxSwipeDistance;
    }

    public static float CalculateOptimalThrowAngleRad(Vector3 ballThrowPosition)
    {
        Vector3 ringCenterPosition = RoundManager.Instance.hoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPosition.x, 0, ringCenterPosition.z));
        float verticalDistance = ringCenterPosition.y - ballThrowPosition.y;

        float minimumAngleRad = 45f * Mathf.Deg2Rad;
        return Mathf.Atan(2 * verticalDistance / horizontalDistance + Mathf.Tan(minimumAngleRad));
    }

    public static float CalculateOptimalThrowPower(Vector3 ballThrowPosition, float optimalAngleRad)
    {
        Vector3 ringCenterPosition = RoundManager.Instance.hoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPosition.x, 0, ringCenterPosition.z));
        float verticalDistance = ringCenterPosition.y - ballThrowPosition.y;

        return Mathf.Sqrt(-Physics.gravity.magnitude * Mathf.Pow(horizontalDistance, 2) /
            (2 * (verticalDistance -
                    horizontalDistance * Mathf.Tan(optimalAngleRad)) *
                Mathf.Pow(Mathf.Cos(optimalAngleRad), 2)));
    }

    public static float CalculateJumpHeight(float jumpForce)
    {
        var initialVelocity = jumpForce;
        var gravity = Physics.gravity.magnitude;
        var jumpHeight = initialVelocity * initialVelocity / (2 * gravity);
        return jumpHeight;
    }
}
