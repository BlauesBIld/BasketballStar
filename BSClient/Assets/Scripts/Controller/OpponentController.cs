using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpponentController : MonoBehaviour
{
    public BallController ballController;
    public Transform positionAboveHead;
    public string opponentName = "Opponent";

    GameObject _scoreBlock;

    public float throwPower;
    public Vector3 throwVector;

    public event Action ThrowEndedEvent;

    public void SetName(string name)
    {
        gameObject.name = name;
        opponentName = name;
    }

    public void SetScoreBlock(GameObject scoreBlock)
    {
        this._scoreBlock = scoreBlock;
    }

    public void UpdateScore(int score)
    {
        _scoreBlock.GetComponentInChildren<TextMeshProUGUI>().text = opponentName + "\n" + score;
    }

    public void SetPosition(Vector3 randomPosition)
    {
        transform.position = randomPosition;
        Vector3 hoopPosition = HoopController.Instance.hoopCenter.position;
        hoopPosition.y = transform.position.y;
        transform.LookAt(hoopPosition);
    }

    public void Jump()
    {
        GetComponent<Rigidbody>()
            .AddForce(Vector3.up * GameManager.Instance.PlayerJumpForce, ForceMode.VelocityChange);
    }

    public void ThrowBall()
    {
        GetComponent<OpponentController>().ballController.Throw(throwVector * throwPower);
    }

    public void SetThrowValues(float throwPower, Vector3 throwVector)
    {
        this.throwPower = throwPower;
        this.throwVector = throwVector;
    }


    private IEnumerator StopChargingAndCallJumpThrowAfterOneSecond()
    {
        yield return new WaitForSeconds(1f);
        JumpThrow();
    }

    private void JumpThrow()
    {
        GetComponent<OpponentController>().Jump();

        StartCoroutine(ThrowWhenVelocityGetsNegative());
    }

    private IEnumerator ThrowWhenVelocityGetsNegative()
    {
        while (GetComponent<Rigidbody>().velocity.y >= 0) yield return null;

        GetComponent<OpponentController>().ThrowBall();

        yield return new WaitForSeconds(GameManager.Instance.ShotFlyingTime);

        ThrowEndedEvent?.Invoke();
    }

    public void StartShooting()
    {
        ballController.StartCharging();
        StartCoroutine(StopChargingAndCallJumpThrowAfterOneSecond());
    }
}