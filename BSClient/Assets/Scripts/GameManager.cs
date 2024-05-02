using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject opponentPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            IgnorePlayerAndOpponentLayerCollisions();
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd GameManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void IgnorePlayerAndOpponentLayerCollisions()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Opponent"), true);
    }

    public void StartSoloGame()
    {
        StartMenuController.Instance.CloseStartMenu();
        RoundManager.Instance.StartRound();
    }

    public void StartRoundAgainstAI()
    {
        StartMenuController.Instance.CloseStartMenu();
        OpponentController opponent = Instantiate(opponentPrefab).GetComponentInChildren<OpponentController>();
        RoundManager.Instance.AddAIOpponent(opponent);
        RoundManager.Instance.StartRound();
    }

    public void ExitToMainMenu()
    {
        EndOfRoundScreenController.Instance.CloseEndOfRoundScreen();
        StartMenuController.Instance.OpenStartMenu();
    }
}