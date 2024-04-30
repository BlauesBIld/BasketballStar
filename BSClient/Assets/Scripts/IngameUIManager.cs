using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameUIManager : MonoBehaviour
{
    public GameObject throwPowerBar;
    public GameObject perfectPowerIndicator;

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
        var power = PlayerController.Instance.ConvertSwipeDistanceToThrowPower();
        throwPowerBar.GetComponent<Slider>().value = power;
    }

    public void SetThrowPowerBarThresholds()
    {
        throwPowerBar.GetComponent<Slider>().maxValue = PlayerController.Instance.highestBackBoardThrowPowerThreshold;
    }

    public void SetPerfectPowerIndicatorPositionAndHeight()
    {
        var perfectPower = PlayerController.Instance.optimalPerfectShotThrowPower;
        var threshold = PlayerController.Instance.perfectShotThreshold;

        var lowerPerfectPowerInPercent = (perfectPower - threshold) / throwPowerBar.GetComponent<Slider>().maxValue;
        var upperPerfectPowerInPercent = (perfectPower + threshold) / throwPowerBar.GetComponent<Slider>().maxValue;

        var rt = throwPowerBar.GetComponent<RectTransform>();
        var maxHeight = rt.rect.height;

        var yPosition = lowerPerfectPowerInPercent * maxHeight;
        var height = (upperPerfectPowerInPercent - lowerPerfectPowerInPercent) * maxHeight;

        perfectPowerIndicator.GetComponent<RectTransform>().position = new Vector3(
            perfectPowerIndicator.GetComponent<RectTransform>().position.x, rt.transform.position.y + yPosition,
            perfectPowerIndicator.GetComponent<RectTransform>().position.z);
        perfectPowerIndicator.GetComponent<RectTransform>().sizeDelta =
            new Vector2(perfectPowerIndicator.GetComponent<RectTransform>().sizeDelta.x, height);
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