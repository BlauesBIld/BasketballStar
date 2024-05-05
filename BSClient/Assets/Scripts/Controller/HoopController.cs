using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopController : MonoBehaviour
{
    public static HoopController Instance { get; private set; }

    public Transform hoopCenter;
    public Transform backBoardHoopCenter;

    public BallController ballController;

    public ParticleSystem rippleEffect;

    private float _physicsErrorMargin = 0.25f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd HoopController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        rippleEffect.Stop();
        AdjustHoopCenterWithRadiusOnYAxis();
        SetBackBoardHoopCenterPosition();
    }
    void AdjustHoopCenterWithRadiusOnYAxis()
    {
        Vector3 hoopCenterPosition = hoopCenter.position;
        Debug.Log("Hoop center position before: " + hoopCenterPosition);
        hoopCenterPosition.y += ballController.GetBallRadius() / 2;
        Debug.Log("Ball radius: " + ballController.GetBallRadius() / 2);
        Debug.Log("Hoop center position adjusted to: " + hoopCenterPosition);
        hoopCenter.position = hoopCenterPosition;
    }

    public void PlayRippleEffect()
    {
        rippleEffect.Play();
    }

    public void SetBackBoardHoopCenterPosition()
    {
        Vector3 backWallPosition = gameObject.transform.position;
        backWallPosition.y = hoopCenter.position.y;
        backWallPosition.x += ballController.GetBallRadius();
        backWallPosition.y += ballController.GetBallRadius() / 2;

        backBoardHoopCenter.position = backWallPosition;
    }
}
