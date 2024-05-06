using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AISelectionScreenController : MonoBehaviour
{
    public static AISelectionScreenController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd AISelectionScreenController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetButtonsText();
    }

    private void SetButtonsText()
    {
        TextMeshProUGUI[] buttonTexts = GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < GameManager.Instance.aiDifficultiesConfigSo.difficulties.Count; i++)
        {
            buttonTexts[i].text = GameManager.Instance.aiDifficultiesConfigSo.difficulties[i].difficultyName;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartMenuController.Instance.OpenStartMenu();
            CloseAISelectionScreen();
        }
    }

    public void CloseAISelectionScreen()
    {
        gameObject.SetActive(false);
    }
}