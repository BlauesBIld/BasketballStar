using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngameUIManager : MonoBehaviour
{
    public static IngameUIManager Instance { get; private set; }
    
    public GameObject throwPowerBar;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd IngameUIManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayerController.Instance.CurrentSwipeDistanceChanged += UpdateThrowPowerBar;
        SetThrowPowerBarThresholds(PlayerController.Instance.lowestThrowPowerThreshold, 30f);
    }

    void Update()
    {
        
    }
    
    public void UpdateThrowPowerBar(float slideDistance)
    {
        float power = slideDistance / 100 + PlayerController.Instance.lowestThrowPowerThreshold;
        throwPowerBar.GetComponent<Slider>().value = power;
    }
    
    public void SetThrowPowerBarThresholds(float min, float max)
    {
        throwPowerBar.GetComponent<Slider>().minValue = min;
        throwPowerBar.GetComponent<Slider>().maxValue = max;
    }
}
