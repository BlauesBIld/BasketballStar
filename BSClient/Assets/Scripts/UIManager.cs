using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd UIManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartMenuController.Instance.OpenStartMenu();
        EndOfRoundScreenController.Instance.CloseEndOfRoundScreen();
    }

    public float GetMaxScreenHeight()
    {
        return Screen.currentResolution.height;
    }
}