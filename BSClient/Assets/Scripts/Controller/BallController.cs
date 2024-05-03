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
        SetPositionNextToOwner();
        ClearTouchedGameObjects();
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
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
                                          owner.transform.right;
        transform.position = ballPositionNextToOwner;
    }

    private void HandlePositionAboveOwner()
    {
        transform.position = positionAboveOwnerHead.position;
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
        MoveBallAboveOwnerInOneSecond();
    }

    private void MoveBallAboveOwnerInOneSecond()
    {
        _rigidbody.isKinematic = true;
        StartCoroutine(MoveBallAboveOwner());
    }

    private IEnumerator MoveBallAboveOwner()
    {
        var time = 0f;
        var duration = 1f;
        Vector3 targetPosition = positionAboveOwnerHead.position;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.position =
                Vector3.Lerp(transform.position, targetPosition, time / duration);
            yield return null;
        }
    }

    public void Throw(Vector3 forceVector)
    {
        _rigidbody.isKinematic = false;
        _ballState = BallStates.Flying;

        _rigidbody.AddForce(forceVector, ForceMode.Impulse);

        var torqueForce = forceVector.magnitude;
        Vector3 torqueDirection = -owner.transform.right;

        _rigidbody.AddTorque(torqueDirection * torqueForce, ForceMode.Impulse);
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