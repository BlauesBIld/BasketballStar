using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultCardController : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    public void SetResultCardValues(string playerName, int score)
    {
        nameText.text = playerName;
        scoreText.text = score.ToString();
    }
}