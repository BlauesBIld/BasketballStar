using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    public static StartMenuController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd StartMenuController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void CloseStartMenu()
    {
        gameObject.SetActive(false);
    }

    public void OpenStartMenu()
    {
        gameObject.SetActive(true);
    }
}