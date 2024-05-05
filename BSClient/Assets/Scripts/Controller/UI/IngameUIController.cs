using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngameUIController : MonoBehaviour
{

    public GameObject fireBallBar;
    public GameObject throwSwipeDistanceBar;
    public GameObject OpponentCorner;
    public GameObject OpponentScoreBlock;

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
        HideIngameUIAndResetFireBallBar();
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

    public void UpdatePlayerScore(int addedScore)
    {
        playerScoreText.text = "Score: \n" + RoundManager.Instance.GetPlayerScore();
    }

    public void UpdateTimer(int timeLeft)
    {
        timerText.text = Mathf.Max(timeLeft, 0).ToString();
        if (timeLeft <= 5)
        {
            //TODO: Slider fill to full for every second
        }
    }

    public void UpdateFireBallBar()
    {
        StartCoroutine(TransitionToValueOnFireBallBar(RoundManager.Instance.consecutiveGoals, 0.3f));
    }

    public void HideIngameUIAndUnsubscribeFromEvents()
    {
        HideIngameUIAndResetFireBallBar();

        RoundManager.Instance.PlayerScoreChangedEvent -= UpdatePlayerScore;
        RoundManager.Instance.OpponentScoreChangedEvent -= UpdateOpponentScore;
        PlayerController.Instance.CurrentSwipeDistanceChangedEvent -= UpdateThrowPowerBar;
    }

    public void HideIngameUIAndResetFireBallBar()
    {
        fireBallBar.GetComponent<Slider>().value = 0;
        gameObject.SetActive(false);
    }

    public void ShowIngameUIAndSubscribeToEvents()
    {
        ShowIngameUI();
        SetThrowSwipeDistanceBarThresholds();
        InstantiateOpponentScoreBlocks();
        RoundManager.Instance.PlayerScoreChangedEvent += UpdatePlayerScore;
        RoundManager.Instance.OpponentScoreChangedEvent += UpdateOpponentScore;
        PlayerController.Instance.CurrentSwipeDistanceChangedEvent += UpdateThrowPowerBar;
    }
    void UpdateOpponentScore(OpponentController opponent, int addedPoints)
    {
        opponent.UpdateScore(RoundManager.Instance.GetOpponents()[opponent]);
    }

    void InstantiateOpponentScoreBlocks()
    {
        var opponents = RoundManager.Instance.GetOpponents();
        float currentRectTransformPosY = 0;
        foreach (OpponentController opponent in opponents.Keys)
        {
            var opponentScoreBlock = Instantiate(OpponentScoreBlock, OpponentCorner.transform);
            opponentScoreBlock.transform.localPosition = new Vector3(0, currentRectTransformPosY, 0);
            opponent.SetScoreBlock(opponentScoreBlock);
            opponent.UpdateScore(0);
            currentRectTransformPosY += opponentScoreBlock.GetComponent<RectTransform>().rect.height;
        }
    }

    public void ShowIngameUI()
    {
        gameObject.SetActive(true);
    }

    private IEnumerator TransitionToValueOnFireBallBar(float value, float duration)
    {
        float elapsedTime = 0;
        var slider = fireBallBar.GetComponent<Slider>();
        var startValue = slider.value;
        while (elapsedTime < duration)
        {
            slider.value = Mathf.Lerp(startValue, value, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        slider.value = value;
    }
    public IEnumerator StartEmptyingFireBallBarAfterItsFullAndWhileFireBallEffectIsActive()
    {
        var slider = fireBallBar.GetComponent<Slider>();
        float duration = 10f;

        yield return new WaitForSeconds(0.5f);

        float elapsedTime = 0;
        var startValue = slider.value;
        while (elapsedTime < duration && RoundManager.Instance.IsFireBallEffectActive())
        {
            slider.value = Mathf.Lerp(startValue, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        RoundManager.Instance.EndFireBallEffect();

        slider.value = 0;
    }
}
