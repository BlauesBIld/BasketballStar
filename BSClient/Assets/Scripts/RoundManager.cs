using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoundManager : MonoBehaviour
{
    public delegate void OnPlayerScoreChanged(int points);

    public Transform hoopCenter;
    public Transform backBoardHoopCenter;

    private readonly float _maxPossibleDistanceFromCenterOfPlayField = 10f;

    private float _roundTime = 15f;
    private bool _hasRoundEnded = true;

    private int _playerScore;
    public GameObject playerPrefab;
    public GameObject opponentPrefab;
    private Dictionary<OpponentController, int> _opponents = new Dictionary<OpponentController, int>();

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
        if (IsRoundActive()) IngameUIController.Instance.UpdateTimer(GetTimeLeft());
    }

    public event OnPlayerScoreChanged PlayerScoreChangedEvent;


    public void StartRound()
    {
        InstantiatePlayer();
        _hasRoundEnded = false;
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

        while (!PlayerController.Instance.ballController.IsDribbling() || !CheckIfOpponentsBallsAreDribbling())
            yield return null;

        SetupEndOfRoundScreen();
        ShowEndOfRoundScreen();
        _hasRoundEnded = true;
        RoundEndedEvent?.Invoke();
    }

    public void ClearDictionaryAndDestroyOpponents()
    {
        foreach (var opponent in _opponents.Keys)
        {
            Destroy(opponent.transform.parent.gameObject);
        }

        _opponents.Clear();
    }

    private void SetupEndOfRoundScreen()
    {
        SetPlayerCardValue();
        SetOpponentsCardValues();

        if (_opponents.Count == 1)
        {
            if (_opponents.Values.First() > _playerScore)
                EndOfRoundScreenController.Instance.ChangeTitleText("You lost!");
            else if (_opponents.Values.First() == _playerScore)
                EndOfRoundScreenController.Instance.ChangeTitleText("Draw!");
            else
                EndOfRoundScreenController.Instance.ChangeTitleText("You won!");
        }
    }

    private void SetOpponentsCardValues()
    {
        foreach (var opponent in _opponents.Keys)
        {
            EndOfRoundScreenController.Instance.InstantiateOpponentCard(opponent.name, _opponents[opponent]);
        }
    }

    private bool CheckIfOpponentsBallsAreDribbling()
    {
        if (_opponents.Count == 0)
        {
            return true;
        }

        foreach (var opponent in _opponents.Keys)
        {
            if (!opponent.ballController.IsDribbling()) return false;
        }

        return true;
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
        var randomPosition = new Vector3(Random.Range(0, _maxPossibleDistanceFromCenterOfPlayField), 2,
            Random.Range(-_maxPossibleDistanceFromCenterOfPlayField, _maxPossibleDistanceFromCenterOfPlayField));
        PlayerController.Instance.transform.position = randomPosition;
        PlayerController.Instance.LookAtHoop();
        CameraController.Instance.SetPositionBehindPlayer();
    }

    public void PlayerShot()
    {
        _playerShotCounter++;
        if (_playerShotCounter % 2 == 0) AssignPlayerToRandomPosition();
    }

    public void AddPointsToPlayer(int points)
    {
        _playerScore += points;
        PlayerScoreChangedEvent?.Invoke(_playerScore);
    }

    public int GetTimeLeft()
    {
        return (int) (_roundTime - (Time.time - _roundStartTimeStamp));
    }

    public Dictionary<OpponentController, int> GetOpponents()
    {
        return _opponents;
    }

    public bool IsRoundActive()
    {
        return Time.time - _roundStartTimeStamp < _roundTime;
    }

    public Vector3 GetCenterOfPlayField()
    {
        Vector3 hoopPosition = hoopCenter.position;
        hoopPosition.y = 0;
        return hoopPosition / 2;
    }

    public float GetRoundTime()
    {
        return _roundTime;
    }

    public float SetRoundTime(float time)
    {
        return _roundTime = time;
    }

    public void AddPointsToOpponent(OpponentController opponent, int points)
    {
        _opponents[opponent] += points;
    }

    public void AddAIOpponent(OpponentController opponent)
    {
        _opponents.Add(opponent, 0);
    }

    public Vector3 GetRandomPositionOnField()
    {
        return new Vector3(
            Random.Range(0, _maxPossibleDistanceFromCenterOfPlayField),
            2,
            Random.Range(-_maxPossibleDistanceFromCenterOfPlayField, _maxPossibleDistanceFromCenterOfPlayField));
    }

    public bool HasRoundEnded()
    {
        return _hasRoundEnded;
    }
}