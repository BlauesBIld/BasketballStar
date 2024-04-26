using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    
    private bool _isChargingThrow = true;
    private float _initialTouchPosition;
    private float? _firstSwipeDirection = null;
    private float _currentSwipeDistance = 0f;
    private float _jumpForce = 8f;
    
    public float lowestThrowPowerThreshold = 10f;
    
    public delegate void OnCurrentSwipeDistanceChanged(float currentSwipeDistance);
    public event OnCurrentSwipeDistanceChanged CurrentSwipeDistanceChanged;
    
    private Rigidbody _rigidbody;

    public BallController ballController;
    public Transform positionAboveHead;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd Player Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        _rigidbody = GetComponent<Rigidbody>();
        HandleSwipeUp();
    }

    private void HandleSwipeUp()
    {
        if (Input.touchCount > 0)
        {
            if (_isChargingThrow)
            {
                if ((_firstSwipeDirection == null || _firstSwipeDirection > 0))
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        _initialTouchPosition = Input.GetTouch(0).position.y;
                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        float swipeDistance = Input.GetTouch(0).position.y - _initialTouchPosition;
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
                            if (_firstSwipeDirection == null)
                            {
                                _firstSwipeDirection = swipeDistance;
                            }
                        }

                        _currentSwipeDistance = Mathf.Max(swipeDistance, 0f);
                        CurrentSwipeDistanceChanged?.Invoke(_currentSwipeDistance);
                    }
                }
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                _firstSwipeDirection = null;
            }
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
        StartCoroutine(ThrowUntilVelocityGetsBelowZero());
    }

    private IEnumerator ThrowUntilVelocityGetsBelowZero()
    {
        while (_rigidbody.velocity.y >= 0)
        {
            Debug.Log("current player velocity: " + _rigidbody.velocity.y);
            yield return null;
        }

        Throw();
        StartCoroutine(ResetShot());
    }

    private IEnumerator ResetShot()
    {
        yield return new WaitForSeconds(3f);
        CurrentSwipeDistanceChanged?.Invoke(0);
        ballController.Reset();
        _isChargingThrow = true;
    }

    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        Debug.Log("Jump");
    }

    private void Throw()
    {
        float throwForce = _currentSwipeDistance/300 + lowestThrowPowerThreshold;
        ballController.Throw(throwForce);
        Debug.Log("Throw with power: " + throwForce);
    }
}