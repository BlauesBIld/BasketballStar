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

    public static float CalculateOptimalBackBoardThrowAngleRad(Vector3 ballThrowPosition)
    {
        Vector3 ringCenterPosition = HoopController.Instance.backBoardHoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPosition.x, 0, ringCenterPosition.z));
        float verticalDistance = ringCenterPosition.y - ballThrowPosition.y;

        float minimumAngleRad = 40f * Mathf.Deg2Rad;
        return Mathf.Atan(2 * verticalDistance / horizontalDistance + Mathf.Tan(minimumAngleRad));
    }

    public static float CalculateOptimalBackBoardThrowPower(Vector3 ballThrowPosition, float optimalAngleRad)
    {
        Vector3 ringCenterPositionForBackBoard = HoopController.Instance.backBoardHoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPositionForBackBoard.x, 0, ringCenterPositionForBackBoard.z));
        float verticalDistance = ringCenterPositionForBackBoard.y - ballThrowPosition.y;

        return Mathf.Sqrt(-Physics.gravity.magnitude * Mathf.Pow(horizontalDistance, 2) /
                          (2 * (verticalDistance -
                                horizontalDistance * Mathf.Tan(optimalAngleRad)) *
                           Mathf.Pow(Mathf.Cos(optimalAngleRad), 2)));
    }

    public static float CalculateOptimalThrowAngleRad(Vector3 ballThrowPosition)
    {
        Vector3 ringCenterPosition = HoopController.Instance.hoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPosition.x, 0, ringCenterPosition.z));
        float verticalDistance = ringCenterPosition.y - ballThrowPosition.y;

        float minimumAngleRad = 45f * Mathf.Deg2Rad;
        return Mathf.Atan(2 * verticalDistance / horizontalDistance + Mathf.Tan(minimumAngleRad));
    }

    public static float CalculateOptimalThrowPower(Vector3 ballThrowPosition, float optimalAngleRad)
    {
        Vector3 ringCenterPosition = HoopController.Instance.hoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPosition.x, 0, ringCenterPosition.z));
        float verticalDistance = ringCenterPosition.y - ballThrowPosition.y;

        return Mathf.Sqrt(-Physics.gravity.magnitude * Mathf.Pow(horizontalDistance, 2) /
                          (2 * (verticalDistance -
                                horizontalDistance * Mathf.Tan(optimalAngleRad)) *
                           Mathf.Pow(Mathf.Cos(optimalAngleRad), 2)));
    }

    public static float CalculateTimeToReachHoop(Vector3 ballThrowPosition, float optimalAngleRad)
    {
        Vector3 ringCenterPosition = HoopController.Instance.hoopCenter.position;

        float horizontalDistance = Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(ringCenterPosition.x, 0, ringCenterPosition.z));

        return horizontalDistance /
               (Mathf.Cos(optimalAngleRad) * CalculateOptimalThrowPower(ballThrowPosition, optimalAngleRad));
    }

    public static float CalculateJumpHeight(float jumpForce)
    {
        float initialVelocity = jumpForce;
        float gravity = Physics.gravity.magnitude;
        float jumpHeight = initialVelocity * initialVelocity / (2 * gravity);
        return jumpHeight;
    }

    public static Vector3 CalculateHorizontalDirectionFromTo(Vector3 fromPosition, Vector3 toPosition)
    {
        fromPosition.y = 0;
        toPosition.y = 0;

        return (toPosition - fromPosition).normalized;
    }
}