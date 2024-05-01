using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfRoundScreenController : MonoBehaviour
{
    public static EndOfRoundScreenController Instance { get; private set; }

    public GameObject playerResultCard;
    public GameObject playersPanel;

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
        gameObject.SetActive(false);
    }
}
