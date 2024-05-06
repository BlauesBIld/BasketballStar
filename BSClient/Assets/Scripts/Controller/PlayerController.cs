using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void OnCurrentSwipeDistanceChanged(float currentSwipeDistance);

    public float optimalPerfectShotThrowPower;
    public float optimalPerfectShotAngleRad;
    public float optimalBackBoardShotThrowPower;
    public float optimalBackBoardShotAngleRad;

    public float PerfectShotThreshold { get; } = 0.08f;
    public float PerfectBackBoardShotThreshold { get; } = 0.05f;

    private float _lowestThrowPowerThreshold;
    private float _highestPerfectThrowPowerThreshold;
    private float _highestBackBoardThrowPowerThreshold;

    public BallController ballController;
    public Transform positionAboveHead;

    private float _currentSwipeDistance;
    private float? _firstSwipeDirection;
    private float _initialTouchPosition = -1;

    private bool _isChargingThrow = true;

    private Rigidbody _rigidbody;

    public event OnCurrentSwipeDistanceChanged CurrentSwipeDistanceChangedEvent;
    public event Action ThrowEndedEvent;
    public event Action ThrowStartedEvent;
    public event Action ThresholdsChangedEvent;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            RoundManager.Instance.RoundCreatedEvent += ResetShot;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd Player Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        ThrowStartedEvent += RoundManager.Instance.AddToPlayerShotCounter;
        ThrowEndedEvent += RoundManager.Instance.AssignPlayerToNewPositionOnEvenShotCounter;
    }

    private void Update()
    {
        HandleSwipeUp();
    }

    private void HandleSwipeUp()
    {
        if (RoundManager.Instance.IsRoundOnGoing() && Input.touchCount > 0)
        {
            if (_isChargingThrow && CheckIfPlayerIsStartingToSwipeOrIsAlreadySwipingUp())
            {
                HandleTouchInput();
            }

            ResetFirstSwipeDirectionIfSwipeEnded();
        }
    }

    private void ResetFirstSwipeDirectionIfSwipeEnded()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Ended) _firstSwipeDirection = null;
    }

    private void HandleTouchInput()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            _initialTouchPosition = Input.GetTouch(0).position.y;
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Moved && _initialTouchPosition > 0f)
        {
            float swipeDistance = Input.GetTouch(0).position.y - _initialTouchPosition;
            if (swipeDistance > 0)
            {
                if (_firstSwipeDirection == null)
                {
                    _firstSwipeDirection = swipeDistance;
                    ballController.StartCharging();
                    StartCoroutine(StopChargingAndCallJumpThrowAfterOneSecond());
                }
            }
            else if (swipeDistance < 0)
            {
                if (_firstSwipeDirection == null) _firstSwipeDirection = swipeDistance;
            }

            _currentSwipeDistance = Mathf.Max(swipeDistance, 0f);
            CurrentSwipeDistanceChangedEvent?.Invoke(_currentSwipeDistance);
        }
    }

    private bool CheckIfPlayerIsStartingToSwipeOrIsAlreadySwipingUp()
    {
        return ((RoundManager.Instance.IsRoundActive() && _firstSwipeDirection == null) ||
                _firstSwipeDirection > 0);
    }

    private IEnumerator StopChargingAndCallJumpThrowAfterOneSecond()
    {
        yield return new WaitForSeconds(1f);
        _isChargingThrow = false;
        JumpThrow();
    }

    private void JumpThrow()
    {
        Jump();
        StartCoroutine(ThrowWhenVelocityGetsNegative());
    }

    private IEnumerator ThrowWhenVelocityGetsNegative()
    {
        while (_rigidbody.velocity.y >= 0) yield return null;

        _currentSwipeDistance = Mathf.Min(_currentSwipeDistance, UIManager.Instance.GetMaxScreenHeight());
        ThrowBall();
        StartCoroutine(ResetShotAfterShootTime());
    }

    public IEnumerator ResetShotAfterShootTime()
    {
        yield return new WaitForSeconds(GameManager.Instance.ShotFlyingTime);
        ResetShot();
    }

    public void ResetShot()
    {
        _currentSwipeDistance = 0f;
        _initialTouchPosition = -1;
        CurrentSwipeDistanceChangedEvent?.Invoke(_currentSwipeDistance);
        ThrowEndedEvent?.Invoke();
        ballController.Reset();
        CalculateAndSetOptimalThrowValues();
        SetThresholds();
        ThrowSwipeDistanceBarController.Instance.SetPerfectPowerIndicatorsPositionAndHeight();
        _isChargingThrow = true;
    }

    private void SetThresholds()
    {
        _lowestThrowPowerThreshold = optimalPerfectShotThrowPower - 1f;
        _highestPerfectThrowPowerThreshold = optimalPerfectShotThrowPower +
                                             (optimalBackBoardShotThrowPower - optimalPerfectShotThrowPower) / 2f;
        _highestBackBoardThrowPowerThreshold = optimalBackBoardShotThrowPower + 0.5f;
        ThresholdsChangedEvent?.Invoke();
    }

    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * GameManager.Instance.PlayerJumpForce, ForceMode.VelocityChange);
    }

    private void ThrowBall()
    {
        ThrowStartedEvent?.Invoke();
        float throwForce = Utils.ConvertSwipeDistanceToThrowPower(_currentSwipeDistance);
        Vector3 throwForceVector = CheckForThresholdsAndCalculateForceVector(throwForce);
        ballController.Throw(throwForceVector);
    }

    private Vector3 CheckForThresholdsAndCalculateForceVector(float throwPower)
    {
        float optimalShotAngleRad;
        Vector3 hoopCenterPosition = HoopController.Instance.hoopCenter.position;
        hoopCenterPosition.y = 0;
        Vector3 backBoardHoopCenterPosition = HoopController.Instance.backBoardHoopCenter.position;
        backBoardHoopCenterPosition.y = 0;
        Vector3 playerPosition = transform.position;
        playerPosition.y = 0;

        Vector3 towardsDesiredHoop;

        if (throwPower <= GetHighestPerfectThrowPower())
        {
            throwPower = CheckAndSetIfPerfectThrow(throwPower);
            optimalShotAngleRad = optimalPerfectShotAngleRad;
            towardsDesiredHoop = (hoopCenterPosition - playerPosition).normalized;
        }
        else
        {
            throwPower = CheckAndSetIfPerfectBackboardThrow(throwPower);
            optimalShotAngleRad = optimalBackBoardShotAngleRad;
            towardsDesiredHoop = (backBoardHoopCenterPosition - playerPosition).normalized;
        }

        Vector3 forceVector = towardsDesiredHoop * Mathf.Cos(optimalShotAngleRad) +
                              transform.up * Mathf.Sin(optimalShotAngleRad);
        return forceVector * throwPower;
    }

    private float CheckAndSetIfPerfectBackboardThrow(float throwPower)
    {
        float maxPerfectBackBoardShotThreshold = optimalBackBoardShotThrowPower + PerfectBackBoardShotThreshold;
        float minPerfectBackBoardShotThreshold = optimalBackBoardShotThrowPower - PerfectBackBoardShotThreshold;

        if (throwPower < maxPerfectBackBoardShotThreshold && throwPower > minPerfectBackBoardShotThreshold)
        {
            Debug.Log("Perfect backboard shot!");
            throwPower = optimalBackBoardShotThrowPower;
        }

        return throwPower;
    }

    private float CheckAndSetIfPerfectThrow(float throwPower)
    {
        float maxPerfectShotThreshold = optimalPerfectShotThrowPower + PerfectShotThreshold;
        float minPerfectShotThreshold = optimalPerfectShotThrowPower - PerfectShotThreshold;

        if (throwPower < maxPerfectShotThreshold && throwPower > minPerfectShotThreshold)
        {
            Debug.Log("Perfect shot!");
            throwPower = optimalPerfectShotThrowPower;
        }

        return throwPower;
    }

    private void CalculateAndSetOptimalThrowValues()
    {
        Vector3 ballThrowPosition = positionAboveHead.position;
        ballThrowPosition.y += Utils.CalculateJumpHeight(GameManager.Instance.PlayerJumpForce);

        optimalPerfectShotAngleRad = Utils.CalculateOptimalThrowAngleRad(ballThrowPosition);
        optimalPerfectShotThrowPower = Utils.CalculateOptimalThrowPower(ballThrowPosition, optimalPerfectShotAngleRad);
        optimalBackBoardShotAngleRad = Utils.CalculateOptimalBackBoardThrowAngleRad(ballThrowPosition);
        optimalBackBoardShotThrowPower =
            Utils.CalculateOptimalBackBoardThrowPower(ballThrowPosition, optimalBackBoardShotAngleRad);
    }

    public void LookAtHoop()
    {
        Vector3 hoopCenterPosition = HoopController.Instance.hoopCenter.position;
        Vector3 lookAtPosition = new Vector3(hoopCenterPosition.x, transform.position.y,
            hoopCenterPosition.z);
        transform.LookAt(lookAtPosition);
    }

    public float GetSlideDistance()
    {
        return _currentSwipeDistance;
    }

    public float GetLowestThrowPower()
    {
        return _lowestThrowPowerThreshold;
    }

    public float GetHighestPerfectThrowPower()
    {
        return _highestPerfectThrowPowerThreshold;
    }

    public float GetThrowPowerRange()
    {
        return _highestBackBoardThrowPowerThreshold - _lowestThrowPowerThreshold;
    }

    void OnDestroy()
    {
        Instance = null;
        RoundManager.Instance.RoundCreatedEvent -= ResetShot;
    }

    public float GetMaxSwipeDistance()
    {
        return UIManager.Instance.GetMaxScreenHeight() / 2;
    }
}