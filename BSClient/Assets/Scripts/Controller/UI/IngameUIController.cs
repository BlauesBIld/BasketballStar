using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngameUIController : MonoBehaviour
{
    public GameObject throwSwipeDistanceBar;

    public RectTransform perfectShotPowerIndicator;

    public RectTransform perfectBackboardShotPowerIndicator;

    public TextMeshProUGUI playerScoreText;

    public TextMeshProUGUI timerText;
    public GameObject timerSlider;
    public static IngameUIController Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            RoundManager.Instance.RoundEndedEvent += HideIngameUIAndUnsubscribeFromEvents;
            RoundManager.Instance.RoundStartedEvent += ShowIngameUIAndSubscribeToEvents;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd IngameUIManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideIngameUI();
    }

    public void UpdateThrowPowerBar(float slideDistance)
    {
        var power = PlayerController.Instance.GetSlideDistance();
        throwSwipeDistanceBar.GetComponent<Slider>().value = power;
    }

    public void SetThrowSwipeDistanceBarThresholds()
    {
        throwSwipeDistanceBar.GetComponent<Slider>().maxValue = PlayerController.Instance.GetMaxSwipeDistance();
    }

    public void SetPerfectPowerIndicatorsPositionAndHeight()
    {
        var perfectShotPower = PlayerController.Instance.optimalPerfectShotThrowPower;
        var perfectBackboardShotPower = PlayerController.Instance.optimalBackBoardShotThrowPower;
        var perfectShotThreshold = PlayerController.Instance.PerfectShotThreshold;
        var perfectBackBoardShotThreshold = PlayerController.Instance.PerfectBackBoardShotThreshold;

        float maxPower = PlayerController.Instance.GetThrowPowerRange();
        float lowestPower = PlayerController.Instance.GetLowestThrowPower();

        var lowerPerfectShotPowerInPercent = (perfectShotPower - perfectShotThreshold - lowestPower) / maxPower;
        var upperPerfectShotPowerInPercent = (perfectShotPower + perfectShotThreshold - lowestPower) / maxPower;

        var lowerPerfectBackboardShotPowerInPercent =
            (perfectBackboardShotPower - perfectBackBoardShotThreshold - lowestPower) / maxPower;
        var upperPerfectBackboardShotPowerInPercent =
            (perfectBackboardShotPower + perfectBackBoardShotThreshold - lowestPower) / maxPower;

        var rt = throwSwipeDistanceBar.GetComponent<RectTransform>();
        var maxHeight = rt.rect.height;

        var perfectShotYPosition = lowerPerfectShotPowerInPercent * maxHeight;
        var perfectShotHeight = (upperPerfectShotPowerInPercent - lowerPerfectShotPowerInPercent) * maxHeight;

        var perfectBackboardShotYPosition = lowerPerfectBackboardShotPowerInPercent * maxHeight;
        var perfectBackboardShotHeight =
            (upperPerfectBackboardShotPowerInPercent - lowerPerfectBackboardShotPowerInPercent) * maxHeight;

        var perfectShotIndicatorPosition = perfectShotPowerIndicator.position;
        perfectShotPowerIndicator.position =
            new Vector3(perfectShotIndicatorPosition.x, rt.transform.position.y + perfectShotYPosition,
                perfectShotIndicatorPosition.z);
        perfectShotPowerIndicator.sizeDelta =
            new Vector2(perfectShotPowerIndicator.sizeDelta.x, perfectShotHeight);

        var perfectBackboardShotIndicatorPosition = perfectBackboardShotPowerIndicator.position;
        perfectBackboardShotPowerIndicator.position =
            new Vector3(perfectBackboardShotIndicatorPosition.x,
                rt.transform.position.y + perfectBackboardShotYPosition,
                perfectBackboardShotIndicatorPosition.z);
        perfectBackboardShotPowerIndicator.sizeDelta =
            new Vector2(perfectShotPowerIndicator.sizeDelta.x, perfectBackboardShotHeight);
    }

    public void UpdatePlayerScore(int score)
    {
        playerScoreText.text = "Score: \n" + score;
    }

    public void UpdateTimer(int timeLeft)
    {
        timerText.text = Mathf.Max(timeLeft, 0).ToString();
        if (timeLeft <= 5)
        {
            //TODO: Slider fill to full for every second
        }
    }

    public void HideIngameUIAndUnsubscribeFromEvents()
    {
        HideIngameUI();

        RoundManager.Instance.PlayerScoreChangedEvent -= UpdatePlayerScore;
        PlayerController.Instance.CurrentSwipeDistanceChangedEvent -= UpdateThrowPowerBar;
    }

    public void HideIngameUI()
    {
        gameObject.SetActive(false);
    }

    public void ShowIngameUIAndSubscribeToEvents()
    {
        ShowIngameUI();
        SetThrowSwipeDistanceBarThresholds();
        RoundManager.Instance.PlayerScoreChangedEvent += UpdatePlayerScore;
        PlayerController.Instance.CurrentSwipeDistanceChangedEvent += UpdateThrowPowerBar;
    }

    public void ShowIngameUI()
    {
        gameObject.SetActive(true);
    }
}