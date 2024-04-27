using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum BallStates
{
    Dribbling,
    Throwing,
    Flying
}

public class BallController : MonoBehaviour
{
    public LineRenderer trajectoryLineRenderer;
    public int resolution = 30; // Number of points in the trajectory line
    public float simulationTime = 2f; // Time in seconds to simulate the trajectory

    
    public PlayerController playerPrefab;
    private BallStates _ballState = BallStates.Dribbling;

    public PhysicMaterial defaultMaterial;
    public PhysicMaterial noBounceMaterial;
    
    private Rigidbody _rigidbody;
    private float _lastBallYVelocity = 0f;
    private float _idleBounceForce = 5f;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        SetPositionNextToPlayer();
        DrawTrajectory();
    }
    
    private void DrawTrajectory()
    {
        trajectoryLineRenderer = GetComponent<LineRenderer>();
        Vector3[] trajectoryPoints = new Vector3[resolution];
        trajectoryLineRenderer.positionCount = resolution;

        Vector3 currentPosition = PlayerController.Instance.positionAboveHead.position;
        
        
        float ratio = PlayerController.Instance.optimalAngleDeg / 90f;
        Vector3 currentVelocity = (playerPrefab.transform.forward * (1 - ratio) + playerPrefab.transform.up * ratio) * PlayerController.Instance.optimalThrowPower;

        float timeStep = simulationTime / resolution;
        for (int i = 0; i < resolution; i++)
        {
            trajectoryPoints[i] = currentPosition;
            currentVelocity += Physics.gravity * timeStep; // Update velocity based on gravity
            currentPosition += currentVelocity * timeStep; // Update position based on velocity
        }

        trajectoryLineRenderer.SetPositions(trajectoryPoints);
    }

    private void SetPositionNextToPlayer()
    {
        Vector3 playerPosition = playerPrefab.transform.position;
        Vector3 ballPosition = new Vector3(playerPosition.x + -0.25f, playerPosition.y, playerPosition.z + 1f);
        transform.position = ballPosition;
    }

    void Update()
    {
        if (_ballState == BallStates.Dribbling)
        {
            HandleIdleBouncing();
        }

        if (_ballState == BallStates.Throwing)
        {
            HandlePositionAbovePlayer();
        }
    }

    private void HandlePositionAbovePlayer()
    {
        //transform.position = playerPrefab.positionAboveHead.position;
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

    public void StartThrowing()
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
        float time = 0f;
        float duration = 1f;
        Vector3 targetPosition = playerPrefab.positionAboveHead.position;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.position =
                Vector3.Lerp(transform.position, targetPosition, time / duration);
            yield return null;
        }
    }

    public void Throw(float throwForce)
    {
        _rigidbody.isKinematic = false;
        _ballState = BallStates.Flying;
        throwForce = PlayerController.Instance.optimalThrowPower;
        float ratio = PlayerController.Instance.optimalAngleDeg / 90f;
        Vector3 forceVector = playerPrefab.transform.forward * (1 - ratio) + playerPrefab.transform.up * ratio;
        _rigidbody.AddForce(forceVector * throwForce, ForceMode.Impulse);
        
        Debug.Log("Throw with power: " + throwForce);
        Debug.Log("Force Vector: " + forceVector);
        
        float torqueForce = throwForce * 14f;
        Vector3 torqueDirection = -playerPrefab.transform.right;
        
        //_rigidbody.AddTorque(torqueDirection * torqueForce, ForceMode.Impulse);
    }

    public void Reset()
    {
        _ballState = BallStates.Dribbling;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        SetPositionNextToPlayer();
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BasketRing"))
        {
            GetComponent<SphereCollider>().material = noBounceMaterial;
            Debug.Log("Ball is not bouncing anymore!");
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BasketRing"))
        {
            GetComponent<SphereCollider>().material = defaultMaterial;
            Debug.Log("Ball is bouncing again!");
        }
    }
    
    
}