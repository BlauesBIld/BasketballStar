using System.Collections;
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

    public void Reset()
    {
        _ballState = BallStates.Dribbling;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        SetPositionNextToPlayer();
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

        if (throwPower < PlayerController.Instance.optimalPerfectShotThrowPower +
            PlayerController.Instance.perfectShotThreshold &&
            throwPower > PlayerController.Instance.optimalPerfectShotThrowPower -
            PlayerController.Instance.perfectShotThreshold)
        {
            throwPower = PlayerController.Instance.optimalPerfectShotThrowPower;
            Debug.Log("Perfect shot!");
        }

        PlayerController player = PlayerController.Instance;
        var optimalAngle = PlayerController.Instance.optimalPerfectShotAngleRad;
        Vector3 forceVector = player.transform.forward * Mathf.Cos(optimalAngle) +
                              player.transform.up * Mathf.Sin(optimalAngle);
        _rigidbody.AddForce(forceVector * throwPower, ForceMode.Impulse);

        Debug.Log("Throw with power: " + throwPower);

        var torqueForce = throwPower * 14f;
        Vector3 torqueDirection = -player.transform.right;

        _rigidbody.AddTorque(torqueDirection * torqueForce, ForceMode.Impulse);
    }

    public bool IsDribbling()
    {
        return _ballState == BallStates.Dribbling;
    }
}