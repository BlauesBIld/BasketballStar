using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngameUIController : MonoBehaviour
{
    public GameObject fireBallBar;
    public GameObject throwSwipeDistanceBar;
    public GameObject OpponentCorner;
    public GameObject OpponentScoreBlock;

    [FormerlySerializedAs("disappearingTextControlle")] [FormerlySerializedAs("disappearingTextController")]
    public GameObject disappearingTextPrefab;

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
            RoundManager.Instance.RoundCreatedEvent += ShowIngameUIAndSubscribeToEvents;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd IngameUIManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void StartAndShowCountDown()
    {
        CountDownController.Instance.StartCountdownAndRoundAfter();
    }

    void Start()
    {
        HideIngameUIAndReset();
    }

    public void UpdateThrowPowerBar(float slideDistance)
    {
        float power = PlayerController.Instance.GetSlideDistance();
        throwSwipeDistanceBar.GetComponent<Slider>().value = power;
    }

    public void SetThrowSwipeDistanceBarThresholds()
    {
        throwSwipeDistanceBar.GetComponent<Slider>().maxValue = PlayerController.Instance.GetMaxSwipeDistance();
    }

    public void SetPerfectPowerIndicatorsPositionAndHeight()
    {
        float perfectShotPower = PlayerController.Instance.optimalPerfectShotThrowPower;
        float perfectBackboardShotPower = PlayerController.Instance.optimalBackBoardShotThrowPower;
        float perfectShotThreshold = PlayerController.Instance.PerfectShotThreshold;
        float perfectBackBoardShotThreshold = PlayerController.Instance.PerfectBackBoardShotThreshold;

        float maxPower = PlayerController.Instance.GetThrowPowerRange();
        float lowestPower = PlayerController.Instance.GetLowestThrowPower();

        float lowerPerfectShotPowerInPercent = (perfectShotPower - perfectShotThreshold - lowestPower) / maxPower;
        float upperPerfectShotPowerInPercent = (perfectShotPower + perfectShotThreshold - lowestPower) / maxPower;

        float lowerPerfectBackboardShotPowerInPercent =
            (perfectBackboardShotPower - perfectBackBoardShotThreshold - lowestPower) / maxPower;
        float upperPerfectBackboardShotPowerInPercent =
            (perfectBackboardShotPower + perfectBackBoardShotThreshold - lowestPower) / maxPower;

        RectTransform rt = throwSwipeDistanceBar.GetComponent<RectTransform>();
        float maxHeight = rt.rect.height;

        float perfectShotYPosition = lowerPerfectShotPowerInPercent * maxHeight;
        float perfectShotHeight = (upperPerfectShotPowerInPercent - lowerPerfectShotPowerInPercent) * maxHeight;

        float perfectBackboardShotYPosition = lowerPerfectBackboardShotPowerInPercent * maxHeight;
        float perfectBackboardShotHeight =
            (upperPerfectBackboardShotPowerInPercent - lowerPerfectBackboardShotPowerInPercent) * maxHeight;

        perfectShotPowerIndicator.localPosition = new Vector3(0, perfectShotYPosition, 0);
        perfectShotPowerIndicator.sizeDelta =
            new Vector2(perfectShotPowerIndicator.sizeDelta.x, perfectShotHeight);

        perfectBackboardShotPowerIndicator.localPosition = new Vector3(0, perfectBackboardShotYPosition, 0);
        perfectBackboardShotPowerIndicator.sizeDelta =
            new Vector2(perfectShotPowerIndicator.sizeDelta.x, perfectBackboardShotHeight);
    }

    public void UpdatePlayerScore(int addedScore)
    {
        playerScoreText.text = "You\n" + RoundManager.Instance.GetPlayerScore();
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
        HideIngameUIAndReset();

        RoundManager.Instance.PlayerScoreChangedEvent -= UpdatePlayerScore;
        RoundManager.Instance.OpponentScoreChangedEvent -= UpdateOpponentScore;
        PlayerController.Instance.CurrentSwipeDistanceChangedEvent -= UpdateThrowPowerBar;
    }

    public void HideIngameUIAndReset()
    {
        fireBallBar.GetComponent<Slider>().value = 0;
        DeleteEveryOpponentUIScoreBlock();
        DeleteAllDisappearingTextsIfAnyExist();

        gameObject.SetActive(false);
    }

    private void DeleteAllDisappearingTextsIfAnyExist()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<DisappearingTextController>())
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void DeleteEveryOpponentUIScoreBlock()
    {
        foreach (RectTransform opponentScoreRect in OpponentCorner.GetComponentsInChildren<RectTransform>())
        {
            if (opponentScoreRect.gameObject != OpponentCorner)
                Destroy(opponentScoreRect.gameObject);
        }
    }

    public void ShowIngameUIAndSubscribeToEvents()
    {
        ShowIngameUI();
        SetThrowSwipeDistanceBarThresholds();
        InstantiateOpponentScoreBlocks();
        StartAndShowCountDown();
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
        Dictionary<OpponentController, int> opponents = RoundManager.Instance.GetOpponents();
        float currentRectTransformPosY = 0;
        foreach (OpponentController opponent in opponents.Keys)
        {
            GameObject opponentScoreBlock = Instantiate(OpponentScoreBlock, OpponentCorner.transform);
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
        Slider slider = fireBallBar.GetComponent<Slider>();
        float startValue = slider.value;
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
        Slider slider = fireBallBar.GetComponent<Slider>();
        float duration = 10f;

        yield return new WaitForSeconds(0.5f);

        float elapsedTime = 0;
        float startValue = slider.value;
        while (elapsedTime < duration && RoundManager.Instance.IsFireBallEffectActive())
        {
            slider.value = Mathf.Lerp(startValue, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        RoundManager.Instance.EndFireBallEffect();

        slider.value = 0;
    }

    public void SpawnDisappearingText(Color color, string title, string subTitle = "")
    {
        DisappearingTextController disappearingText =
            Instantiate(disappearingTextPrefab, transform).GetComponent<DisappearingTextController>();
        disappearingText.SetText(title, subTitle);
        disappearingText.SetColor(color);
    }
}