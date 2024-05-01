using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private OpponentController _opponentController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd GameManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    public void StartSoloGame()
    {
        StartMenuController.Instance.CloseStartMenu();
        RoundManager.Instance.StartRound();
    }

    public void ExitToMainMenu()
    {
        EndOfRoundScreenController.Instance.CloseEndOfRoundScreen();
        StartMenuController.Instance.OpenStartMenu();
    }
}
