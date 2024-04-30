using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void OnCurrentSwipeDistanceChanged(float currentSwipeDistance);

    public float optimalPerfectShotThrowPower;
    public float optimalPerfectShotAngleRad;
    public float perfectShotThreshold = 1f;

    public float lowestThrowPowerThreshold;
    public float highestPerfectThrowPowerThreshold;
    public float highestBackBoardThrowPowerThreshold;

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
        CalculateOptimalThrowPowers();
    }

    private void Update()
    {
        HandleSwipeUp();
    }

    private void OnEnable()
    {
        ballController.enabled = true;
    }

    private void OnDisable()
    {
        ballController.enabled = false;
    }

    public event OnCurrentSwipeDistanceChanged CurrentSwipeDistanceChangedEvent;
    public event Action ThrowEndedEvent;
    public event Action ThresholdsChangedEvent;

    private void HandleSwipeUp()
    {
        if (Input.touchCount > 0)
        {
            if (_isChargingThrow)
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
                                ballController.StartThrowing();
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
        Throw();
        StartCoroutine(ResetShotAfterShootTime());
    }

    public IEnumerator ResetShotAfterShootTime()
    {
        yield return new WaitForSeconds(2f);
        ResetShot();
    }

    public void ResetShot()
    {
        _currentSwipeDistance = 0f;
        CurrentSwipeDistanceChangedEvent?.Invoke(_currentSwipeDistance);
        ThrowEndedEvent?.Invoke();
        ballController.Reset();
        CalculateOptimalThrowPowers();
        SetThresholds();
        IngameUIManager.Instance.SetPerfectPowerIndicatorPositionAndHeight();
        _isChargingThrow = true;
    }

    private void SetThresholds()
    {
        lowestThrowPowerThreshold = optimalPerfectShotThrowPower / 2;
        highestPerfectThrowPowerThreshold = optimalPerfectShotThrowPower + 2f;
        highestBackBoardThrowPowerThreshold = optimalPerfectShotThrowPower * 1.5f +
                                              (GetHorizontalDistanceFromCenterToHoop() -
                                               GetHorizontalDistanceFromHoop());
        ThresholdsChangedEvent?.Invoke();
    }

    private float GetHorizontalDistanceFromCenterToHoop()
    {
        return new Vector3(RoundManager.Instance.hoopCenter.position.x, 0, RoundManager.Instance.hoopCenter.position.z)
            .magnitude;
    }

    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
    }

    private void Throw()
    {
        Debug.Log("Current Swipe Distance: " + _currentSwipeDistance);
        Debug.Log("maxSwipeDistance: " + _maxSwipeDistance);
        Debug.Log("ConvertSwipeDistanceToThrowPower: " + ConvertSwipeDistanceToThrowPower());
        var throwForce = Mathf.Max(ConvertSwipeDistanceToThrowPower(), lowestThrowPowerThreshold);
        ballController.Throw(throwForce);
    }

    private void CalculateOptimalThrowPowers()
    {
        var ballThrowPosition = positionAboveHead.position;
        ballThrowPosition.y += CalculatJumpHeight();

        var ringCenterPosition = RoundManager.Instance.hoopCenter.position;

        var horizontalDistance = GetHorizontalDistanceFromHoop();
        var verticalDistance = ringCenterPosition.y - ballThrowPosition.y;

        var minimumAngleRad = 45f * Mathf.Deg2Rad;
        var optimalAngleRadCalculated =
            Mathf.Atan(2 * verticalDistance / horizontalDistance + Mathf.Tan(minimumAngleRad));

        var optimalThrowPowerCalculated = Mathf.Sqrt(-Physics.gravity.magnitude * Mathf.Pow(horizontalDistance, 2) /
                                                     (2 * (verticalDistance -
                                                           horizontalDistance * Mathf.Tan(optimalAngleRadCalculated)) *
                                                      Mathf.Pow(Mathf.Cos(optimalAngleRadCalculated), 2)));
        Debug.Log("Optimal Throw Power: " + optimalThrowPowerCalculated);

        optimalPerfectShotThrowPower = optimalThrowPowerCalculated;
        optimalPerfectShotAngleRad = optimalAngleRadCalculated;
    }

    public float CalculatJumpHeight()
    {
        var initialVelocity = _jumpForce;
        var gravity = Physics.gravity.magnitude;
        var jumpHeight = initialVelocity * initialVelocity / (2 * gravity);
        return jumpHeight;
    }

    public void LookAtHoop()
    {
        var lookAtPosition = new Vector3(RoundManager.Instance.hoopCenter.position.x, transform.position.y,
            RoundManager.Instance.hoopCenter.position.z);
        transform.LookAt(lookAtPosition);
    }

    public float GetHorizontalDistanceFromHoop()
    {
        return Vector3.Distance(new Vector3(positionAboveHead.position.x, 0, positionAboveHead.position.z),
            new Vector3(RoundManager.Instance.hoopCenter.position.x, 0, RoundManager.Instance.hoopCenter.position.z));
    }

    public float GetHorizontalDistanceFromBackBoardHoop()
    {
        return Vector3.Distance(new Vector3(positionAboveHead.position.x, 0, positionAboveHead.position.z),
            new Vector3(RoundManager.Instance.backBoardHoopCenter.position.x, 0,
                RoundManager.Instance.backBoardHoopCenter.position.z));
    }

    public float ConvertSwipeDistanceToThrowPower()
    {
        return _currentSwipeDistance * 100 / _maxSwipeDistance * highestBackBoardThrowPowerThreshold / 100;
    }
}