using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoundManager : MonoBehaviour
{
    public delegate void OnPlayerScoreChanged(int addedPoints);

    public delegate void OnOpponentScoreChanged(OpponentController opponent, int addedPoints);

    private readonly float _maxPossibleDistanceFromCenterOfPlayField = 5f;

    private float _roundDuration = 15f;
    private bool _hasRoundEnded = true;

    private int _playerScore;
    public GameObject playerPrefab;
    public GameObject opponentPrefab;

    private readonly Dictionary<OpponentController, int>
        _opponentsAndScores = new Dictionary<OpponentController, int>();

    private int _playerShotCounter;
    private float _roundStartTimeStamp;

    public int consecutiveGoals = 0;
    private int _lastShotPlayerScored = 0;
    private bool _fireBallBonusActive = false;

    public event OnPlayerScoreChanged PlayerScoreChangedEvent;
    public event OnOpponentScoreChanged OpponentScoreChangedEvent;
    public event Action RoundEndedEvent;
    public event Action RoundStartedEvent;
    public event Action RoundCreatedEvent;

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
        if (HasRoundStarted() && IsRoundActive()) IngameUIController.Instance.UpdateTimer(GetTimeLeft());
    }


    public void CreateRound()
    {
        InstantiatePlayer();
        ResetVariableValues();
        AssignPlayerToRandomPosition();
        RoundCreatedEvent?.Invoke();
        PlayerScoreChangedEvent?.Invoke(0);
    }

    public void StartRound()
    {
        StartCoroutine(EndGameAfterTime());
        RoundStartedEvent?.Invoke();
    }

    void ResetVariableValues()
    {
        _hasRoundEnded = false;
        _fireBallBonusActive = false;
        _playerScore = 0;
        _playerShotCounter = 0;
        _lastShotPlayerScored = 0;
        consecutiveGoals = 0;
    }

    void InstantiatePlayer()
    {
        Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
    }

    private IEnumerator EndGameAfterTime()
    {
        _roundStartTimeStamp = Time.time;
        while (Time.time - _roundStartTimeStamp < _roundDuration)
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
        foreach (OpponentController opponent in _opponentsAndScores.Keys)
        {
            Destroy(opponent.transform.parent.gameObject);
        }

        _opponentsAndScores.Clear();
    }

    private void SetupEndOfRoundScreen()
    {
        SetPlayerCardValue();
        SetOpponentsCardValues();

        if (_opponentsAndScores.Count == 1)
        {
            if (_opponentsAndScores.Values.First() > _playerScore)
                EndOfRoundScreenController.Instance.ChangeTitleText("You lost!");
            else if (_opponentsAndScores.Values.First() == _playerScore)
                EndOfRoundScreenController.Instance.ChangeTitleText("Draw!");
            else
                EndOfRoundScreenController.Instance.ChangeTitleText("You won!");
        }
    }

    private void SetOpponentsCardValues()
    {
        if (_opponentsAndScores.Count > 0)
            EndOfRoundScreenController.Instance.playersPanel.SetActive(true);
        else
            EndOfRoundScreenController.Instance.playersPanel.SetActive(false);

        foreach (OpponentController opponent in _opponentsAndScores.Keys)
        {
            EndOfRoundScreenController.Instance.InstantiateOpponentCard(opponent.name, _opponentsAndScores[opponent]);
        }
    }

    private bool CheckIfOpponentsBallsAreDribbling()
    {
        if (_opponentsAndScores.Count == 0)
        {
            return true;
        }

        foreach (OpponentController opponent in _opponentsAndScores.Keys)
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
        var randomPosition = GetRandomPositionOnField();
        PlayerController.Instance.transform.position = randomPosition;
        PlayerController.Instance.LookAtHoop();
        CameraController.Instance.SetPositionBehindPlayer();
    }

    public void AddToPlayerShotCounter()
    {
        _playerShotCounter++;
    }

    public void AssignPlayerToNewPositionOnEvenShotCounter()
    {
        if (_playerShotCounter > _lastShotPlayerScored)
        {
            consecutiveGoals = 0;
            _fireBallBonusActive = false;
        }

        if (!IsFireBallEffectActive())
        {
            IngameUIController.Instance.UpdateFireBallBar();
            SetFireBallBonusActive();
        }

        if (_playerShotCounter % 2 == 0) AssignPlayerToRandomPosition();
    }

    void SetFireBallBonusActive()
    {
        if (consecutiveGoals >= 3)
        {
            _fireBallBonusActive = true;
            StartCoroutine(IngameUIController.Instance
                .StartEmptyingFireBallBarAfterItsFullAndWhileFireBallEffectIsActive());
        }
        else
        {
            _fireBallBonusActive = false;
        }
    }

    public void AddPointsToPlayerAndSpawnDisappearingText(int points)
    {
        SetColorAndSubtitleText(points, out Color textColor, out string subTitle);

        if (_fireBallBonusActive)
        {
            points *= 2;
        }

        string title = "+" + points + " pts!";
        IngameUIController.Instance.SpawnDisappearingText(textColor, title, subTitle);

        _playerScore += points;
        _lastShotPlayerScored = _playerShotCounter;
        consecutiveGoals++;
        PlayerScoreChangedEvent?.Invoke(points);
    }

    private static void SetColorAndSubtitleText(int points, out Color textColor, out string subTitle)
    {
        switch (points)
        {
            case 3:
                textColor = Color.green;
                subTitle = "Perfect Shot!";
                break;
            case 6:
                textColor = Color.cyan;
                subTitle = "Backboard Shot!";
                break;
            default:
                textColor = Color.HSVToRGB(0.08f, 0.8f, 1);
                subTitle = "";
                break;
        }
    }

    public int GetTimeLeft()
    {
        return (int) (_roundDuration - (Time.time - _roundStartTimeStamp));
    }

    public Dictionary<OpponentController, int> GetOpponents()
    {
        return _opponentsAndScores;
    }

    public bool IsRoundActive()
    {
        return Time.time - _roundStartTimeStamp < _roundDuration;
    }

    public Vector3 GetCenterOfPlayField()
    {
        Vector3 hoopPosition = HoopController.Instance.hoopCenter.position;
        hoopPosition.y = 0;
        return hoopPosition / 2;
    }

    public float GetRoundDuration()
    {
        return _roundDuration;
    }

    public float SetRoundDuration(float time)
    {
        return _roundDuration = time;
    }

    public void AddPointsToOpponent(OpponentController opponent, int points)
    {
        _opponentsAndScores[opponent] += points;
        OpponentScoreChangedEvent?.Invoke(opponent, points);
    }

    public void AddAIOpponent(int aiDifficultyNumber)
    {
        OpponentController opponent = Instantiate(opponentPrefab).GetComponentInChildren<OpponentController>();
        DifficultySetting aiDifficulty = GameManager.Instance.aiDifficultiesConfigSo.difficulties[aiDifficultyNumber];
        opponent.GetComponent<AIController>().SetDifficultyParams(aiDifficulty);
        opponent.SetName(aiDifficulty.opponentName);
        _opponentsAndScores.Add(opponent, 0);
    }

    public Vector3 GetRandomPositionOnField()
    {
        float minDistanceOnXFromCenter = 7f;
        float maxDistanceOnXFromCenter = 5f + _maxPossibleDistanceFromCenterOfPlayField;
        return new Vector3(
            Random.Range(minDistanceOnXFromCenter, maxDistanceOnXFromCenter),
            0,
            Random.Range(-_maxPossibleDistanceFromCenterOfPlayField, _maxPossibleDistanceFromCenterOfPlayField));
    }

    public bool HasRoundEnded()
    {
        return _hasRoundEnded;
    }

    public bool HasRoundStarted()
    {
        return _roundStartTimeStamp != 0;
    }

    public bool IsRoundOnGoing()
    {
        return HasRoundStarted() && !HasRoundEnded();
    }

    public int GetPlayerScore()
    {
        return _playerScore;
    }

    public bool IsFireBallEffectActive()
    {
        return _fireBallBonusActive;
    }

    public void EndFireBallEffect()
    {
        _fireBallBonusActive = false;
        consecutiveGoals = 0;
    }
}