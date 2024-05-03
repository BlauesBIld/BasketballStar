using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : MonoBehaviour
{
    public Transform positionAboveHead;

    private float _jumpForce = 8f;
    public float ShotFlyingTime { get; } = 2f;

    public float optimalPerfectShotThrowPower;
    public float optimalPerfectShotAngleRad;
    public float optimalBackBoardShotThrowPower;
    public float optimalBackBoardShotAngleRad;

    private float _maxDelayBeforeThrow = 1.5f;
    private float _minDelayBeforeThrow = 0.5f;

    private float _maxErrorPower = 0.5f;

    private void Awake()
    {
        RoundManager.Instance.RoundStartedEvent += ResetShot;
        RoundManager.Instance.RoundStartedEvent += StartShooting;
    }

    private void Start()
    {
        GetComponent<OpponentController>().SetName("Computer");
    }

    private void StartShooting()
    {
        StartCoroutine(ShootWithRandomDelay());
    }

    private IEnumerator ShootWithRandomDelay()
    {
        yield return new WaitForSeconds(Random.Range(_minDelayBeforeThrow, _maxDelayBeforeThrow));
        if (RoundManager.Instance.IsRoundActive())
        {
            GetComponent<OpponentController>().ballController.StartCharging();
            StartCoroutine(StopChargingAndCallJumpThrowAfterOneSecond());
        }
    }

    private IEnumerator StopChargingAndCallJumpThrowAfterOneSecond()
    {
        yield return new WaitForSeconds(1f);
        JumpThrow();
    }

    private void JumpThrow()
    {
        Jump();
        StartCoroutine(ThrowWhenVelocityGetsNegative());
    }

    private IEnumerator ThrowWhenVelocityGetsNegative()
    {
        while (GetComponent<Rigidbody>().velocity.y >= 0) yield return null;

        ThrowBall();
        StartCoroutine(ResetShotAfterShootTime());
    }

    private void ThrowBall()
    {
        var throwPower = optimalPerfectShotThrowPower + Random.Range(-_maxErrorPower, _maxErrorPower);
        Vector3 hoopPosition = RoundManager.Instance.hoopCenter.position;
        hoopPosition.y = 0;
        Vector3 opponentPosition = transform.position;
        opponentPosition.y = 0;

        Vector3 towardsDesiredHoop = (hoopPosition - opponentPosition).normalized;
        Vector3 throwVector = towardsDesiredHoop * Mathf.Cos(optimalPerfectShotAngleRad) +
                              transform.up * Mathf.Sin(optimalPerfectShotAngleRad);
        GetComponent<OpponentController>().ballController.Throw(throwVector * throwPower);
    }

    public IEnumerator ResetShotAfterShootTime()
    {
        yield return new WaitForSeconds(ShotFlyingTime);
        ResetShot();
        StartCoroutine(ShootWithRandomDelay());
    }

    private void Jump()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
    }

    public void ResetShot()
    {
        SetRandomPositionOnField();
        GetComponent<OpponentController>().ballController.Reset();
        CalculateAndSetOptimalThrowValues();
    }

    private void SetRandomPositionOnField()
    {
        Vector3 randomPosition = RoundManager.Instance.GetRandomPositionOnField();
        transform.position = randomPosition;
        Vector3 hoopPosition = RoundManager.Instance.hoopCenter.position;
        hoopPosition.y = transform.position.y;
        transform.LookAt(hoopPosition);
    }

    private void CalculateAndSetOptimalThrowValues()
    {
        var ballThrowPosition = positionAboveHead.position;
        ballThrowPosition.y += Utils.CalculateJumpHeight(_jumpForce);

        optimalPerfectShotAngleRad = Utils.CalculateOptimalThrowAngleRad(ballThrowPosition);
        optimalPerfectShotThrowPower = Utils.CalculateOptimalThrowPower(ballThrowPosition, optimalPerfectShotAngleRad);
        optimalBackBoardShotAngleRad = Utils.CalculateOptimalBackBoardThrowAngleRad(ballThrowPosition);
        optimalBackBoardShotThrowPower =
            Utils.CalculateOptimalBackBoardThrowPower(ballThrowPosition, optimalBackBoardShotAngleRad);
    }

    private void OnDestroy()
    {
        RoundManager.Instance.RoundStartedEvent -= ResetShot;
        RoundManager.Instance.RoundStartedEvent -= StartShooting;
    }
}