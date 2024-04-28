using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private OpponentController opponentController;
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd GameManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerController.Instance.enabled = false;
    }

    public void StartSoloGame()
    {
        StartMenuController.Instance.CloseStartMenu();
        RoundManager.Instance.StartRound();
    }
}
