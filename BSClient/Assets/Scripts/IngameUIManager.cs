using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngameUIManager : MonoBehaviour
{
    public GameObject throwSwipeDistanceBar;
    public RectTransform perfectPowerIndicator;

    public TextMeshProUGUI playerScoreText;

    public TextMeshProUGUI timerText;
    public GameObject timerSlider;
    public static IngameUIManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd IngameUIManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerController.Instance.CurrentSwipeDistanceChangedEvent += UpdateThrowPowerBar;
        PlayerController.Instance.ThresholdsChangedEvent += SetThrowPowerBarThresholds;
    }

    public void UpdateThrowPowerBar(float slideDistance)
    {
        var power = PlayerController.Instance.GetSlideDistance();
        throwSwipeDistanceBar.GetComponent<Slider>().value = power;
    }

    public void SetThrowPowerBarThresholds()
    {
        throwSwipeDistanceBar.GetComponent<Slider>().maxValue = PlayerController.Instance.GetMaxSwipeDistance();
    }

    public void SetPerfectPowerIndicatorPositionAndHeight()
    {
        var perfectPower = PlayerController.Instance.optimalPerfectShotThrowPower;
        var threshold = PlayerController.Instance.perfectShotThreshold;

        float maxPower = PlayerController.Instance.highestBackBoardThrowPowerThreshold;

        var lowerPerfectPowerInPercent = (perfectPower - threshold) / maxPower;
        var upperPerfectPowerInPercent = (perfectPower + threshold) / maxPower;

        var rt = throwSwipeDistanceBar.GetComponent<RectTransform>();
        var maxHeight = rt.rect.height;

        var yPosition = lowerPerfectPowerInPercent * maxHeight;
        var height = (upperPerfectPowerInPercent - lowerPerfectPowerInPercent) * maxHeight;

        var indicatorPosition = perfectPowerIndicator.position;
        perfectPowerIndicator.position = new Vector3(indicatorPosition.x, rt.transform.position.y + yPosition, indicatorPosition.z);
        perfectPowerIndicator.sizeDelta =
            new Vector2(perfectPowerIndicator.sizeDelta.x, height);
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

    public void HideIngameUI()
    {
        gameObject.SetActive(false);
    }

    public void ShowIngameUI()
    {
        gameObject.SetActive(true);
    }
}
