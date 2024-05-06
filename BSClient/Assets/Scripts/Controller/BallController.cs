using System;
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
    public GameObject owner;
    public Transform positionAboveOwnerHead;

    public PhysicMaterial defaultMaterial;
    public PhysicMaterial noBounceMaterial;
    private readonly float _idleBounceForce = 3f;
    private BallStates _ballState = BallStates.Dribbling;
    private float _lastBallYVelocity;

    private HashSet<GameObject> _touchedGameObjects = new HashSet<GameObject>();

    public void Reset()
    {
        _ballState = BallStates.Dribbling;
        Rigidbody ballRigidbody = GetComponent<Rigidbody>();
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        SetPositionNextToOwner();
        ClearTouchedGameObjects();
    }

    private void Start()
    {
        SetPositionNextToOwner();
    }

    private void Update()
    {
        if (_ballState == BallStates.Dribbling) HandleIdleBouncing();

        if (_ballState == BallStates.Throwing) HandlePositionAboveOwner();
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

    private void SetPositionNextToOwner()
    {
        Vector3 ballPositionNextToOwner = owner.transform.position + owner.transform.forward * 0.25f +
                                          owner.transform.right * 0.5f;
        transform.position = ballPositionNextToOwner;
    }

    private void HandlePositionAboveOwner()
    {
        transform.position = positionAboveOwnerHead.position;
    }

    private void HandleIdleBouncing()
    {
        Rigidbody ballRigidbody = GetComponent<Rigidbody>();
        if (_lastBallYVelocity < 0 && ballRigidbody.velocity.y >= 0)
        {
            ballRigidbody.velocity = new Vector3(ballRigidbody.velocity.x, 0, ballRigidbody.velocity.z);
            ballRigidbody.AddForce(Vector3.up * _idleBounceForce, ForceMode.Impulse);
        }

        _lastBallYVelocity = ballRigidbody.velocity.y;
    }

    public void StartCharging()
    {
        _ballState = BallStates.Throwing;
        MoveBallAboveOwnerInOneSecond();
    }

    private void MoveBallAboveOwnerInOneSecond()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(MoveBallAboveOwner());
    }

    private IEnumerator MoveBallAboveOwner()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = positionAboveOwnerHead.position;
        float time = 0;
        float duration = 0.4f;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
    }

    public void Throw(Vector3 forceVector)
    {
        Rigidbody ballRigidbody = GetComponent<Rigidbody>();
        ballRigidbody.isKinematic = false;
        _ballState = BallStates.Flying;

        ballRigidbody.AddForce(forceVector, ForceMode.Impulse);

        float torqueForce = forceVector.magnitude;
        Vector3 torqueDirection = -owner.transform.right;

        ballRigidbody.AddTorque(torqueDirection * torqueForce, ForceMode.Impulse);
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

    public float GetBallRadius()
    {
        return GetComponent<SphereCollider>().radius * transform.localScale.x;
    }
}