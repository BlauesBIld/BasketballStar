using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoundManager : MonoBehaviour
{
    public Transform hoopCenter;
    public Transform backBoardHoopCenter;

    private readonly float _maxDistanceFromCenterOfPlayField = 10f;
    private readonly float _roundTime = 60f;

    private bool _isRoundActive;

    private int _playerScore;

    private float _playerShotCounter;
    private float _roundStartTimeStamp;

    public static RoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Tried to create 2nd RoundManager Instance. Deleting it instead.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerController.Instance.OnThrowEnded += PlayerShot;
        PlayerController.Instance.ResetShotAfterShootTime();
    }

    private void Update()
    {
        if (_isRoundActive) IngameUIManager.Instance.UpdateTimer(GetTimeLeft());
    }

    public void StartRound()
    {
        _isRoundActive = true;
        AssignPlayerToRandomPosition();
        StartCoroutine(EndGameAfterTime());
        PlayerController.Instance.enabled = true;
        PlayerController.Instance.ResetShot();
    }

    private IEnumerator EndGameAfterTime()
    {
        _roundStartTimeStamp = Time.time;
        Debug.Log("Round started at: " + _roundStartTimeStamp);
        Debug.Log("Round will take: " + _roundTime + " seconds.");
        Debug.Log("Round will end at: " + (_roundStartTimeStamp + _roundTime));
        while (Time.time - _roundStartTimeStamp < _roundTime) yield return null;

        Debug.Log("Round ended with so many points: " + _playerScore);
        PlayerController.Instance.enabled = false;
        //TODO: Show end game screen
        _isRoundActive = false;
    }

    private void AssignPlayerToRandomPosition()
    {
        var randomPosition = new Vector3(Random.Range(0, _maxDistanceFromCenterOfPlayField), 2,
            Random.Range(-_maxDistanceFromCenterOfPlayField, _maxDistanceFromCenterOfPlayField));
        PlayerController.Instance.transform.position = randomPosition;
        PlayerController.Instance.LookAtHoop();
    }

    public void PlayerShot()
    {
        _playerShotCounter++;
        if (_playerShotCounter % 2 == 0) AssignPlayerToRandomPosition();
    }

    public void AddPointsToPlayerScore(int points)
    {
        _playerScore += points;
    }

    public int GetTimeLeft()
    {
        return (int) (_roundTime - (Time.time - _roundStartTimeStamp));
    }
}