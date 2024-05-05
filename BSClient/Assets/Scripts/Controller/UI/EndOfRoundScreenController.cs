using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndOfRoundScreenController : MonoBehaviour
{
    public static EndOfRoundScreenController Instance { get; private set; }

    public GameObject playerResultCard;
    public GameObject playersPanel;
    public TextMeshProUGUI titleText;

    public GameObject playerCardPrefab;

    private List<GameObject> _playerCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd EndOfRoundScreenController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    public void OpenEndOfRoundScreen()
    {
        gameObject.SetActive(true);
        if (RoundManager.Instance.GetOpponents().Count <= 0)
        {
            playersPanel.SetActive(false);
        }
    }

    public void CloseEndOfRoundScreen()
    {
        if (PlayerController.Instance != null)
        {
            Destroy(PlayerController.Instance.transform.parent.gameObject);
        }

        RoundManager.Instance.ClearDictionaryAndDestroyOpponents();

        ChangeTitleText("Game Over!");
        gameObject.SetActive(false);
    }

    public void InstantiateOpponentCard(string opponentName, int score)
    {
        GameObject opponentCard = Instantiate(playerCardPrefab, playersPanel.transform);
        opponentCard.transform.localScale *= 0.8f;
        opponentCard.GetComponent<ResultCardController>().SetResultCardValues(opponentName, score);
    }

    public void ChangeTitleText(string text)
    {
        titleText.text = text;
    }
}