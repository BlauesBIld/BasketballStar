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
}