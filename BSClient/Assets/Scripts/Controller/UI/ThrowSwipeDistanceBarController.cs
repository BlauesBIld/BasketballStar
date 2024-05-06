using UnityEngine;
using UnityEngine.UI;

public class ThrowSwipeDistanceBarController : MonoBehaviour
{
    public static ThrowSwipeDistanceBarController Instance { get; private set; }

    public RectTransform perfectShotPowerIndicator;
    public RectTransform perfectBackboardShotPowerIndicator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd ThrowSwipeDistanceBarController Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    public void SetPerfectPowerIndicatorsPositionAndHeight()
    {
        float perfectShotPower = PlayerController.Instance.optimalPerfectShotThrowPower;
        float perfectShotThreshold = PlayerController.Instance.PerfectShotThreshold;
        float perfectBackboardShotPower = PlayerController.Instance.optimalBackBoardShotThrowPower;
        float perfectBackBoardShotThreshold = PlayerController.Instance.PerfectBackBoardShotThreshold;

        float maxPower = PlayerController.Instance.GetThrowPowerRange();
        float lowestPower = PlayerController.Instance.GetLowestThrowPower();

        float lowerPerfectShotPowerInPercent = (perfectShotPower - perfectShotThreshold - lowestPower) / maxPower;
        float upperPerfectShotPowerInPercent = (perfectShotPower + perfectShotThreshold - lowestPower) / maxPower;

        float lowerPerfectBackboardShotPowerInPercent =
            (perfectBackboardShotPower - perfectBackBoardShotThreshold - lowestPower) / maxPower;
        float upperPerfectBackboardShotPowerInPercent =
            (perfectBackboardShotPower + perfectBackBoardShotThreshold - lowestPower) / maxPower;

        RectTransform rt = GetComponent<RectTransform>();
        float maxHeight = rt.rect.height;

        float perfectShotYPosition = lowerPerfectShotPowerInPercent * maxHeight - maxHeight / 2;
        float perfectShotHeight = (upperPerfectShotPowerInPercent - lowerPerfectShotPowerInPercent) * maxHeight;

        float perfectBackboardShotYPosition = lowerPerfectBackboardShotPowerInPercent * maxHeight - maxHeight / 2;
        float perfectBackboardShotHeight =
            (upperPerfectBackboardShotPowerInPercent - lowerPerfectBackboardShotPowerInPercent) * maxHeight;

        perfectShotPowerIndicator.localPosition = new Vector3(0, perfectShotYPosition, 0);
        perfectShotPowerIndicator.sizeDelta =
            new Vector2(perfectShotPowerIndicator.sizeDelta.x, perfectShotHeight);

        perfectBackboardShotPowerIndicator.localPosition = new Vector3(0, perfectBackboardShotYPosition, 0);
        perfectBackboardShotPowerIndicator.sizeDelta =
            new Vector2(perfectShotPowerIndicator.sizeDelta.x, perfectBackboardShotHeight);
    }

    public void UpdateThrowPowerBar(float slideDistance)
    {
        float power = PlayerController.Instance.GetSlideDistance();
        GetComponent<Slider>().value = power;
    }

    public void SetThrowSwipeDistanceBarThresholds()
    {
        GetComponent<Slider>().maxValue = PlayerController.Instance.GetMaxSwipeDistance();
    }
}