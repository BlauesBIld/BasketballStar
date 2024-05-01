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

    public float PerfectShotThreshold { get; } = 0.22f;
    public float PerfectBackBoardShotThreshold { get; } = 0.13f;
    public float ShotFlyingTime { get; } = 2f;

    private float _lowestThrowPowerThreshold;
    private float _highestPerfectThrowPowerThreshold;
    private float _highestBackBoardThrowPowerThreshold;

    public BallController ballController;
    public Transform positionAboveHead;

    private readonly float _jumpForce = 8f;
    private readonly float _maxSwipeDistance = Screen.height / 2f;
    private float _currentSwipeDistance;
    private float? _firstSwipeDirection;
    private float _initialTouchPosition;

    private bool _isChargingThrow = true;

    private Rigidbody _rigidbody;
    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        ThrowEndedEvent += RoundManager.Instance.PlayerShot;
    }

    private void Update()
    {
        HandleSwipeUp();
    }

    public event OnCurrentSwipeDistanceChanged CurrentSwipeDistanceChangedEvent;
    public event Action ThrowEndedEvent;
    public event Action ThrowStartedEvent;
    public event Action ThresholdsChangedEvent;

    private void HandleSwipeUp()
    {
        if (RoundManager.Instance.IsRoundActive() && Input.touchCount > 0)
        {
            if (_isChargingThrow)
            {
                if (_firstSwipeDirection == null || _firstSwipeDirection > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        _initialTouchPosition = Input.GetTouch(0).position.y;
                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        var swipeDistance = Input.GetTouch(0).position.y - _initialTouchPosition;
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
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended) _firstSwipeDirection = null;
        }
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

        _currentSwipeDistance = Mathf.Min(_currentSwipeDistance, _maxSwipeDistance);
        ThrowBall();
        StartCoroutine(ResetShotAfterShootTime());
    }

    public IEnumerator ResetShotAfterShootTime()
    {
        yield return new WaitForSeconds(ShotFlyingTime);
        ResetShot();
    }

    public void ResetShot()
    {
        _currentSwipeDistance = 0f;
        CurrentSwipeDistanceChangedEvent?.Invoke(_currentSwipeDistance);
        ThrowEndedEvent?.Invoke();
        ballController.Reset();
        CalculateAndSetOptimalThrowValues();
        SetThresholds();
        IngameUIManager.Instance.SetPerfectPowerIndicatorsPositionAndHeight();
        _isChargingThrow = true;
    }

    private void SetThresholds()
    {
        _lowestThrowPowerThreshold = optimalPerfectShotThrowPower - 1.7f - GetHorizontalDistanceFromHoop() * 0.1f;
        _highestPerfectThrowPowerThreshold = optimalPerfectShotThrowPower +
                                             (optimalBackBoardShotThrowPower - optimalPerfectShotThrowPower) / 2f;
        _highestBackBoardThrowPowerThreshold =
            optimalBackBoardShotThrowPower + (3f - GetHorizontalDistanceFromHoop() * 0.1f);
        ThresholdsChangedEvent?.Invoke();
    }

    private float GetHorizontalDistanceFromCenterToHoop()
    {
        var hoopPosition = RoundManager.Instance.hoopCenter.position;
        return new Vector3(hoopPosition.x, 0, hoopPosition.z)
            .magnitude;
    }

    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
    }

    private void ThrowBall()
    {
        ThrowStartedEvent?.Invoke();
        var throwForce = Utils.ConvertSwipeDistanceToThrowPower(_currentSwipeDistance);
        ballController.Throw(throwForce);
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

    public void LookAtHoop()
    {
        Vector3 hoopCenterPosition = RoundManager.Instance.hoopCenter.position;
        Vector3 lookAtPosition = new Vector3(hoopCenterPosition.x, transform.position.y,
            hoopCenterPosition.z);
        transform.LookAt(lookAtPosition);
    }

    public float GetHorizontalDistanceFromHoop()
    {
        Vector3 hoopCenterPosition = RoundManager.Instance.hoopCenter.position;
        Vector3 throwPosition = positionAboveHead.position;
        return Vector3.Distance(new Vector3(throwPosition.x, 0, throwPosition.z),
            new Vector3(hoopCenterPosition.x, 0, hoopCenterPosition.z));
    }

    public float GetHorizontalDistanceFromBackBoardHoop()
    {
        var ballThrowPosition = positionAboveHead.position;
        var hoopCenterPosition = RoundManager.Instance.backBoardHoopCenter.position;
        return Vector3.Distance(new Vector3(ballThrowPosition.x, 0, ballThrowPosition.z),
            new Vector3(hoopCenterPosition.x, 0, hoopCenterPosition.z));
    }

    public float GetSlideDistance()
    {
        return _currentSwipeDistance;
    }

    public float GetMaxSwipeDistance()
    {
        return _maxSwipeDistance;
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
        RoundManager.Instance.RoundStartedEvent -= ResetShot;
    }
}