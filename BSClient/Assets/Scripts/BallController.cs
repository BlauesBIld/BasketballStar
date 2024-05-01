using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallStates
{
    Dribbling,
    Throwing,
    Flying
}

public class BallController : MonoBehaviour
{
    public PhysicMaterial defaultMaterial;
    public PhysicMaterial noBounceMaterial;
    private readonly float _idleBounceForce = 10f;
    private BallStates _ballState = BallStates.Dribbling;
    private float _lastBallYVelocity;

    private Rigidbody _rigidbody;

    private HashSet<GameObject> _touchedGameObjects = new HashSet<GameObject>();

    public void Reset()
    {
        _ballState = BallStates.Dribbling;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        SetPositionNextToPlayer();
        ClearTouchedGameObjects();
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        SetPositionNextToPlayer();
    }

    private void Update()
    {
        if (_ballState == BallStates.Dribbling) HandleIdleBouncing();

        if (_ballState == BallStates.Throwing) HandlePositionAbovePlayer();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BounceChanger"))
        {
            GetComponent<SphereCollider>().material = noBounceMaterial;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BounceChanger"))
        {
            GetComponent<SphereCollider>().material = defaultMaterial;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("BackBoard") ||
            other.gameObject.layer == LayerMask.NameToLayer("Hoop"))
        {
            _touchedGameObjects.Add(other.gameObject);
        }
    }

    private void SetPositionNextToPlayer()
    {
        PlayerController player = PlayerController.Instance;
        Vector3 ballPositionNextToPlayer = player.transform.position + player.transform.forward * 0.25f +
                                           player.transform.right;
        transform.position = ballPositionNextToPlayer;
    }

    private void HandlePositionAbovePlayer()
    {
        transform.position = PlayerController.Instance.positionAboveHead.position;
    }

    private void HandleIdleBouncing()
    {
        if (_lastBallYVelocity < 0 && _rigidbody.velocity.y >= 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * _idleBounceForce, ForceMode.Impulse);
        }

        _lastBallYVelocity = _rigidbody.velocity.y;
    }

    public void StartCharging()
    {
        _ballState = BallStates.Throwing;
        MoveBallAbovePlayerInOneSecond();
    }

    private void MoveBallAbovePlayerInOneSecond()
    {
        _rigidbody.isKinematic = true;
        StartCoroutine(MoveBallAbovePlayer());
    }

    private IEnumerator MoveBallAbovePlayer()
    {
        var time = 0f;
        var duration = 1f;
        Vector3 targetPosition = PlayerController.Instance.positionAboveHead.position;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.position =
                Vector3.Lerp(transform.position, targetPosition, time / duration);
            yield return null;
        }
    }

    public void Throw(float throwPower)
    {
        _rigidbody.isKinematic = false;
        _ballState = BallStates.Flying;

        var forceVector = CheckForThresholdsAndCalculateForceVector(ref throwPower, out var player);
        _rigidbody.AddForce(forceVector * throwPower, ForceMode.Impulse);

        var torqueForce = throwPower * 14f;
        Vector3 torqueDirection = -player.transform.right;

        _rigidbody.AddTorque(torqueDirection * torqueForce, ForceMode.Impulse);
    }

    private static Vector3 CheckForThresholdsAndCalculateForceVector(ref float throwPower, out PlayerController player)
    {
        float perfectShotOrBackBoardShotThreshold = PlayerController.Instance.GetHighestPerfectThrowPower();
        float optimalShotAngleRad = 0f;
        Vector3 hoopCenterPosition = RoundManager.Instance.hoopCenter.position;
        hoopCenterPosition.y = 0;
        Vector3 backBoardHoopCenterPosition = RoundManager.Instance.backBoardHoopCenter.position;
        backBoardHoopCenterPosition.y = 0;
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        playerPosition.y = 0;

        Vector3 towardsDesiredHoop = Vector3.zero;

        if (throwPower <= perfectShotOrBackBoardShotThreshold)
        {
            throwPower = CheckAndSetIfPerfectThrow(throwPower);
            optimalShotAngleRad = PlayerController.Instance.optimalPerfectShotAngleRad;
            towardsDesiredHoop = (hoopCenterPosition - playerPosition).normalized;
        }
        else
        {
            throwPower = CheckAndSetIfPerfectBackboardThrow(throwPower);
            optimalShotAngleRad = PlayerController.Instance.optimalBackBoardShotAngleRad;
            towardsDesiredHoop = (backBoardHoopCenterPosition - playerPosition).normalized;
        }

        player = PlayerController.Instance;
        Vector3 forceVector = towardsDesiredHoop * Mathf.Cos(optimalShotAngleRad) +
                              player.transform.up * Mathf.Sin(optimalShotAngleRad);
        return forceVector;
    }

    private static float CheckAndSetIfPerfectBackboardThrow(float throwPower)
    {
        float maxPerfectBackBoardShotThreshold = PlayerController.Instance.optimalBackBoardShotThrowPower +
                                                 PlayerController.Instance.PerfectBackBoardShotThreshold;
        float minPerfectBackBoardShotThreshold = PlayerController.Instance.optimalBackBoardShotThrowPower -
                                                 PlayerController.Instance.PerfectBackBoardShotThreshold;

        if (throwPower < maxPerfectBackBoardShotThreshold && throwPower > minPerfectBackBoardShotThreshold)
        {
            Debug.Log("Perfect Backboard shot!");
            throwPower = PlayerController.Instance.optimalBackBoardShotThrowPower;
        }

        return throwPower;
    }

    private static float CheckAndSetIfPerfectThrow(float throwPower)
    {
        float maxPerfectShotThreshold = PlayerController.Instance.optimalPerfectShotThrowPower +
                                        PlayerController.Instance.PerfectShotThreshold;
        float minPerfectShotThreshold = PlayerController.Instance.optimalPerfectShotThrowPower -
                                        PlayerController.Instance.PerfectShotThreshold;

        if (throwPower < maxPerfectShotThreshold && throwPower > minPerfectShotThreshold)
        {
            Debug.Log("Perfect shot!");
            throwPower = PlayerController.Instance.optimalPerfectShotThrowPower;
        }

        return throwPower;
    }

    public HashSet<GameObject> GetTouchedGameObjects()
    {
        return _touchedGameObjects;
    }

    public bool IsDribbling()
    {
        return _ballState == BallStates.Dribbling;
    }

    private void ClearTouchedGameObjects()
    {
        _touchedGameObjects.Clear();
    }
}