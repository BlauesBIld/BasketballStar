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
    public PlayerController playerPrefab;
    private BallStates _ballState = BallStates.Dribbling;

    private Rigidbody _rigidbody;
    private float lastBallYVelocity = 0f;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        SetPositionNextToPlayer();
    }

    private void SetPositionNextToPlayer()
    {
        Vector3 playerPosition = playerPrefab.transform.position;
        Vector3 ballPosition = new Vector3(playerPosition.x + -0.15f, playerPosition.y, playerPosition.z + 1f);
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
        transform.position = playerPrefab.positionAboveHead.position;
    }

    private void HandleIdleBouncing()
    {
        if (lastBallYVelocity < 0 && _rigidbody.velocity.y >= 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        lastBallYVelocity = _rigidbody.velocity.y;
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
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.position =
                Vector3.Lerp(transform.position, playerPrefab.positionAboveHead.position, time / duration);
            yield return null;
        }
    }

    public void Throw(float throwForce)
    {
        _rigidbody.isKinematic = false;
        _ballState = BallStates.Flying;
        _rigidbody.AddForce((playerPrefab.transform.up*2.5f + playerPrefab.transform.forward) * 3f, ForceMode.Impulse);
    }
}