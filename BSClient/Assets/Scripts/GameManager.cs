using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public AIDifficultiesConfigSO aiDifficultiesConfigSo;
    public float PlayerJumpForce { get; } = 3f;
    public float ShotFlyingTime { get; } = 2f;

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

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void IgnorePlayerAndOpponentLayerCollisions()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Opponent"), true);
    }

    public void StartSoloGame()
    {
        StartMenuController.Instance.CloseStartMenu();
        RoundManager.Instance.CreateRound();
    }

    public void StartRoundAgainstAI(int aiDifficulty)
    {
        StartMenuController.Instance.CloseStartMenu();
        AISelectionScreenController.Instance.CloseAISelectionScreen();
        RoundManager.Instance.AddAIOpponent(aiDifficulty);
        RoundManager.Instance.CreateRound();
    }

    public void ExitToMainMenu()
    {
        EndOfRoundScreenController.Instance.CloseEndOfRoundScreen();
        StartMenuController.Instance.OpenStartMenu();
    }
}