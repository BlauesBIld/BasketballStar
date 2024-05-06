using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private float _initialDistanceFromCenter = 0f;
    private float _distanceFromHoop = 8f;
    private float _fixedCameraHeight = 7f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd CameraController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RoundManager.Instance.RoundStartedEvent += BindToPlayer;
        RoundManager.Instance.RoundEndedEvent += UnbindFromPlayer;
        ResetPosition();
    }

    void ResetPosition()
    {
        transform.position = new Vector3(-8, _fixedCameraHeight, 0);
        _initialDistanceFromCenter = Vector3.Distance(transform.position, RoundManager.Instance.GetCenterOfPlayField());
    }

    void Update()
    {
        if (RoundManager.Instance.HasRoundEnded())
        {
            RotateAroundFieldCenter();
        }
    }

    void RotateAroundFieldCenter()
    {
        Transform cameraTransform = transform;
        cameraTransform.position = new Vector3(transform.position.x, _fixedCameraHeight, transform.position.z);
        cameraTransform.position += cameraTransform.right * Time.deltaTime * 2f;
        Vector3 centerOfPlayField = RoundManager.Instance.GetCenterOfPlayField();
        centerOfPlayField.y = 3f;
        cameraTransform.LookAt(centerOfPlayField);
        float deltaDistance =
            Vector3.Distance(cameraTransform.position, centerOfPlayField) - _initialDistanceFromCenter;
        cameraTransform.position += cameraTransform.forward * deltaDistance;
    }

    public void BindToPlayer()
    {
        PlayerController.Instance.ThrowStartedEvent += MoveBetweenPlayerAndHoop;
        SetPositionBehindPlayer();
    }

    public void UnbindFromPlayer()
    {
        PlayerController.Instance.ThrowStartedEvent -= MoveBetweenPlayerAndHoop;
        ResetPosition();
    }

    public void SetPositionBehindPlayer()
    {
        Transform playerTransform = PlayerController.Instance.transform;
        Vector3 playerPosition = playerTransform.position;
        playerPosition.y += 2.5f;
        transform.position = playerPosition - playerTransform.forward * 6f;
        transform.rotation = playerTransform.rotation;
    }

    public void MoveBetweenPlayerAndHoop()
    {
        Vector3 hoopPosition = HoopController.Instance.hoopCenter.position;
        Vector3 targetPosition = (transform.position - hoopPosition).normalized * _distanceFromHoop + hoopPosition;
        targetPosition.y = hoopPosition.y;
        Vector3 targetUpPosition = transform.position + Vector3.up * 2f;

        StartCoroutine(MoveBetweenPlayerAndHoopSequence(targetUpPosition, targetPosition));
    }

    IEnumerator MoveBetweenPlayerAndHoopSequence(Vector3 targetUpPosition, Vector3 targetPosition)
    {
        float timeForThrow = Utils.CalculateTimeToReachHoop(PlayerController.Instance.transform.position,
            PlayerController.Instance.optimalPerfectShotAngleRad);
        targetPosition.y += 1f;
        yield return StartCoroutine(MoveToPosition(targetUpPosition, timeForThrow * 0.7f));

        float timeDifference = GameManager.Instance.ShotFlyingTime - timeForThrow * 0.7f;
        targetPosition.y += 1f;
        yield return StartCoroutine(MoveToPosition(targetPosition, timeDifference));

        SetPositionBehindPlayer();
    }


    IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            if (transform.position.y > HoopController.Instance.hoopCenter.position.y)
                transform.LookAt(HoopController.Instance.hoopCenter.position);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}