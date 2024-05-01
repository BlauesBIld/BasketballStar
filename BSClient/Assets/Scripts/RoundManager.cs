using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoundManager : MonoBehaviour
{
    public delegate void OnPlayerScoreChanged(int points);

    public Transform hoopCenter;
    public Transform backBoardHoopCenter;

    private readonly float _maxDistanceFromCenterOfPlayField = 10f;
    private readonly float _roundTime = 7f;

    private bool _isRoundActive;

    private int _playerScore;
    public GameObject playerPrefab;
    public GameObject opponentPrefab;
    private List<OpponentController> _opponents = new List<OpponentController>();

    private float _playerShotCounter;
    private float _roundStartTimeStamp;

    public event Action RoundEndedEvent;
    public event Action RoundStartedEvent;

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

    private void Update()
    {
        if (_isRoundActive) IngameUIManager.Instance.UpdateTimer(GetTimeLeft());
    }

    public event OnPlayerScoreChanged PlayerScoreChangedEvent;


    public void StartRound()
    {
        InstantiatePlayer();
        RoundStartedEvent += PlayerController.Instance.ResetShot;
        _isRoundActive = true;
        _playerScore = 0;
        AssignPlayerToRandomPosition();
        StartCoroutine(EndGameAfterTime());
        RoundStartedEvent?.Invoke();
        PlayerScoreChangedEvent?.Invoke(_playerScore);
    }

    void InstantiatePlayer()
    {
        Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
    }

    private IEnumerator EndGameAfterTime()
    {
        _roundStartTimeStamp = Time.time;
        while (Time.time - _roundStartTimeStamp < _roundTime)
            yield return null;

        while (!PlayerController.Instance.ballController.IsDribbling())
            yield return null;

        SetPlayerCardValue();
        ShowEndOfRoundScreen();
        _isRoundActive = false;
        RoundEndedEvent?.Invoke();
    }

    private void SetPlayerCardValue()
    {
        EndOfRoundScreenController.Instance.playerResultCard.GetComponent<ResultCardController>()
            .SetResultCardValues("You", _playerScore);
    }

    private void ShowEndOfRoundScreen()
    {
        EndOfRoundScreenController.Instance.OpenEndOfRoundScreen();
    }

    private void AssignPlayerToRandomPosition()
    {
        var randomPosition = new Vector3(Random.Range(0, _maxDistanceFromCenterOfPlayField), 2,
            Random.Range(-_maxDistanceFromCenterOfPlayField, _maxDistanceFromCenterOfPlayField));
        PlayerController.Instance.transform.position = randomPosition;
        PlayerController.Instance.LookAtHoop();
        CameraController.Instance.BindToPlayer();
    }

    public void PlayerShot()
    {
        _playerShotCounter++;
        if (_playerShotCounter % 2 == 0) AssignPlayerToRandomPosition();
    }

    public void AddPointsToPlayerScore(int points)
    {
        _playerScore += points;
        PlayerScoreChangedEvent?.Invoke(_playerScore);
    }

    public int GetTimeLeft()
    {
        return (int)(_roundTime - (Time.time - _roundStartTimeStamp));
    }

    public List<OpponentController> GetOpponents()
    {
        return _opponents;
    }
    public bool IsRoundActive()
    {
        return _isRoundActive;
    }
    public Vector3 GetCenterOfPlayField()
    {
        Vector3 hoopPosition = hoopCenter.position;
        hoopPosition.y = 0;
        return hoopPosition / 2;
    }
}
