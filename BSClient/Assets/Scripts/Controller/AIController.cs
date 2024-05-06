using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : MonoBehaviour
{
    public float ShotFlyingTime { get; } = 2f;

    private float _optimalPerfectShotThrowPower;
    private float _optimalPerfectShotAngleRad;
    private float _optimalBackBoardShotThrowPower;
    private float _optimalBackBoardShotAngleRad;

    private float _maxDelayBeforeThrow = 1.5f;
    private float _minDelayBeforeThrow = 0.5f;

    private float _maxErrorPower = 0.1f;

    private float _chanceOfThrowingBackBoardShot = 0.12f;

    private void Awake()
    {
        RoundManager.Instance.RoundCreatedEvent += ResetShot;
        RoundManager.Instance.RoundStartedEvent += StartShooting;
    }

    private void Start()
    {
        GetComponent<OpponentController>().ThrowEndedEvent += ResetShotAndStartShootingWithRandomDelayAgain;
    }

    private void StartShooting()
    {
        StartCoroutine(ShootWithRandomDelay());
    }

    private IEnumerator ShootWithRandomDelay()
    {
        yield return new WaitForSeconds(Random.Range(_minDelayBeforeThrow, _maxDelayBeforeThrow));

        if (BackBoardController.Instance.IsGlowing()) _chanceOfThrowingBackBoardShot = 0.6f;
        DecidedIfBackBoardShotAndSetValuesOnOpponentController();


        if (RoundManager.Instance.IsRoundActive())
        {
            GetComponent<OpponentController>().StartShooting();
        }
    }

    private void DecidedIfBackBoardShotAndSetValuesOnOpponentController()
    {
        Vector3 towardsDesiredHoop;
        float throwPower;
        Vector3 throwVector;

        if (Random.value < _chanceOfThrowingBackBoardShot)
        {
            towardsDesiredHoop = Utils.CalculateHorizontalDirectionFromTo(transform.position,
                HoopController.Instance.backBoardHoopCenter.position);
            throwPower = _optimalBackBoardShotThrowPower + Random.Range(-_maxErrorPower, _maxErrorPower);
            throwVector = towardsDesiredHoop * Mathf.Cos(_optimalBackBoardShotAngleRad) +
                          transform.up * Mathf.Sin(_optimalBackBoardShotAngleRad);

            Debug.Log("Backboard shot");
        }
        else
        {
            towardsDesiredHoop = Utils.CalculateHorizontalDirectionFromTo(transform.position,
                HoopController.Instance.hoopCenter.position);
            throwPower = _optimalPerfectShotThrowPower + Random.Range(-_maxErrorPower, _maxErrorPower);
            throwVector = towardsDesiredHoop * Mathf.Cos(_optimalPerfectShotAngleRad) +
                          transform.up * Mathf.Sin(_optimalPerfectShotAngleRad);

            Debug.Log("Perfect shot");
        }

        GetComponent<OpponentController>().SetThrowValues(throwPower, throwVector);
    }

    public void ResetShotAndStartShootingWithRandomDelayAgain()
    {
        ResetShot();
        StartCoroutine(ShootWithRandomDelay());
    }

    public void ResetShot()
    {
        _chanceOfThrowingBackBoardShot = 0.12f;
        SetRandomPositionOnField();
        GetComponent<OpponentController>().ballController.Reset();
        CalculateAndSetOptimalThrowValues();
    }

    private void SetRandomPositionOnField()
    {
        Vector3 randomPosition = RoundManager.Instance.GetRandomPositionOnField();
        GetComponent<OpponentController>().SetPosition(randomPosition);
    }

    private void CalculateAndSetOptimalThrowValues()
    {
        Vector3 ballThrowPosition = GetComponent<OpponentController>().positionAboveHead.position;
        ballThrowPosition.y += Utils.CalculateJumpHeight(GameManager.Instance.PlayerJumpForce);

        _optimalPerfectShotAngleRad = Utils.CalculateOptimalThrowAngleRad(ballThrowPosition);
        _optimalPerfectShotThrowPower =
            Utils.CalculateOptimalThrowPower(ballThrowPosition, _optimalPerfectShotAngleRad);
        _optimalBackBoardShotAngleRad = Utils.CalculateOptimalBackBoardThrowAngleRad(ballThrowPosition);
        _optimalBackBoardShotThrowPower =
            Utils.CalculateOptimalBackBoardThrowPower(ballThrowPosition, _optimalBackBoardShotAngleRad);
    }

    private void OnDestroy()
    {
        RoundManager.Instance.RoundCreatedEvent -= ResetShot;
        RoundManager.Instance.RoundStartedEvent -= StartShooting;
    }

    public void SetDifficultyParams(DifficultySetting aiDifficulty)
    {
        _maxDelayBeforeThrow = aiDifficulty.maxDelayBeforeThrow;
        _minDelayBeforeThrow = aiDifficulty.minDelayBeforeThrow;
        _maxErrorPower = aiDifficulty.maxThrowPowerError;
    }
}